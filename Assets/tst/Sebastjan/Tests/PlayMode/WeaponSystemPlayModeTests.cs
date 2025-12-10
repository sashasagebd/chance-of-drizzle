using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    /// <summary>
    /// Category 2: Weapon System Core Tests (PlayMode)
    /// Tests that require play mode for timing, events, and fire rate limiting
    /// </summary>
    public class WeaponSystemPlayModeTests
    {
        private GameObject weaponObject;
        private TestWeapon weapon;

        // Helper class to test abstract WeaponBase
        private class TestWeapon : WeaponBase
        {
            public bool fireWasCalled = false;
            public int fireCallCount = 0;
            public int dryFireCount = 0;

            protected override bool DoFire(Vector3 origin, Vector3 direction)
            {
                fireWasCalled = true;
                fireCallCount++;
                return true;
            }

            protected override void OnDryFire()
            {
                base.OnDryFire();
                dryFireCount++;
            }

            public void ResetFlags()
            {
                fireWasCalled = false;
                fireCallCount = 0;
                dryFireCount = 0;
            }
        }

        [UnitySetUp]
        public IEnumerator Setup()
        {
            weaponObject = new GameObject("TestWeapon");
            weapon = weaponObject.AddComponent<TestWeapon>();

            // Configure weapon
            weapon.magazineSize = 10;
            weapon.fireRate = 10f; // 10 shots per second = 0.1s between shots
            weapon.damage = 25;

            yield return null; // Wait for Awake
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (weaponObject != null)
                Object.Destroy(weaponObject);

            yield return null;
        }

        #region Test 8: Fire Rate Limiting
        /// <summary>
        /// Test 8: Fire Rate Limiting
        /// Verifies weapon respects fire rate and doesn't fire faster than configured
        /// Bug Detection: Would catch if fire rate limiting is broken
        /// </summary>
        [UnityTest]
        public IEnumerator Test08_FireRateLimiting_PreventsRapidFire()
        {
            // Arrange - Fire rate is 10/sec = 0.1s between shots
            weapon.fireRate = 10f;

            // Act - Try to fire twice immediately
            bool firstShot = weapon.TryFire(Vector3.zero, Vector3.forward);
            bool secondShot = weapon.TryFire(Vector3.zero, Vector3.forward);

            // Assert
            Assert.IsTrue(firstShot, "First shot should succeed");
            Assert.IsFalse(secondShot, "Second immediate shot should be blocked by fire rate");
            Assert.AreEqual(1, weapon.fireCallCount, "DoFire should only be called once");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Test08_FireRateLimiting_AllowsFireAfterCooldown()
        {
            // Arrange
            weapon.fireRate = 10f; // 0.1s cooldown

            // Act - Fire first shot
            weapon.TryFire(Vector3.zero, Vector3.forward);
            weapon.ResetFlags();

            // Wait for cooldown (0.1s + buffer)
            yield return new WaitForSeconds(0.15f);

            // Try second shot
            bool secondShot = weapon.TryFire(Vector3.zero, Vector3.forward);

            // Assert
            Assert.IsTrue(secondShot, "Should allow firing after cooldown period");
            Assert.IsTrue(weapon.fireWasCalled, "DoFire should be called after cooldown");
        }

        [UnityTest]
        public IEnumerator Test08_FireRateLimiting_RespectsDifferentFireRates()
        {
            // Arrange - Slow fire rate (2 shots per second = 0.5s between shots)
            weapon.fireRate = 2f;

            // Act
            weapon.TryFire(Vector3.zero, Vector3.forward);
            weapon.ResetFlags();

            // Wait insufficient time (0.3s < 0.5s)
            yield return new WaitForSeconds(0.3f);
            bool tooEarly = weapon.TryFire(Vector3.zero, Vector3.forward);

            // Wait total 0.6s (enough for cooldown)
            yield return new WaitForSeconds(0.3f);
            bool afterCooldown = weapon.TryFire(Vector3.zero, Vector3.forward);

            // Assert
            Assert.IsFalse(tooEarly, "Should not fire at 0.3s (cooldown is 0.5s)");
            Assert.IsTrue(afterCooldown, "Should fire at 0.6s (after 0.5s cooldown)");
        }

        [UnityTest]
        public IEnumerator Test08_FireRateLimiting_DoesNotAffectAmmo()
        {
            // Arrange
            int initialAmmo = weapon.ammo;

            // Act - Try rapid fire (blocked by rate limit)
            weapon.TryFire(Vector3.zero, Vector3.forward);
            weapon.TryFire(Vector3.zero, Vector3.forward); // Blocked
            weapon.TryFire(Vector3.zero, Vector3.forward); // Blocked

            yield return null;

            // Assert
            Assert.AreEqual(initialAmmo - 1, weapon.ammo,
                "Ammo should only decrease once (blocked shots don't consume ammo)");
        }
        #endregion

        #region Test 17: Weapon Fire Event
        [UnityTest]
        public IEnumerator Test17_WeaponFireEvent_OnAmmoChangedOnReload()
        {
            // Arrange
            int eventCallCount = 0;
            weapon.ammo = 5;

            weapon.OnAmmoChanged += (ammo, magazineSize, reserve) =>
            {
                eventCallCount++;
            };

            // Act
            weapon.Reload();
            yield return null;

            // Assert
            Assert.AreEqual(1, eventCallCount, "OnAmmoChanged should fire on reload");
        }

        [UnityTest]
        public IEnumerator Test17_WeaponFireEvent_MultipleSubscribers()
        {
            // Arrange
            int subscriber1Calls = 0;
            int subscriber2Calls = 0;

            weapon.OnAmmoChanged += (a, m, r) => subscriber1Calls++;
            weapon.OnAmmoChanged += (a, m, r) => subscriber2Calls++;

            // Act
            weapon.TryFire(Vector3.zero, Vector3.forward);
            yield return null;

            // Assert
            Assert.AreEqual(1, subscriber1Calls, "First subscriber should be notified");
            Assert.AreEqual(1, subscriber2Calls, "Second subscriber should be notified");
        }
        #endregion

        #region Test 18: Dry Fire
        /// <summary>
        /// Test 18: Dry Fire Behavior
        /// Verifies weapon handles firing with no ammo (dry fire)
        /// Bug Detection: Would catch if dry fire crashes or behaves incorrectly
        /// </summary>
        [UnityTest]
        public IEnumerator Test18_DryFire_TriggersWhenOutOfAmmo()
        {
            // Arrange - Deplete all ammo
            weapon.ammo = 0;

            // Act
            bool fired = weapon.TryFire(Vector3.zero, Vector3.forward);
            yield return null;

            // Assert
            Assert.IsFalse(fired, "TryFire should return false on dry fire");
            Assert.AreEqual(1, weapon.dryFireCount, "OnDryFire should be called once");
            Assert.AreEqual(0, weapon.fireCallCount, "DoFire should NOT be called");
        }

        [UnityTest]
        public IEnumerator Test18_DryFire_MultipleDryFireAttempts()
        {
            // Arrange
            weapon.ammo = 0;

            // Act - Try multiple dry fires
            weapon.TryFire(Vector3.zero, Vector3.forward);
            yield return new WaitForSeconds(0.15f); // Wait for fire rate cooldown
            weapon.TryFire(Vector3.zero, Vector3.forward);
            yield return new WaitForSeconds(0.15f);
            weapon.TryFire(Vector3.zero, Vector3.forward);
            yield return null;

            // Assert
            Assert.AreEqual(3, weapon.dryFireCount, "OnDryFire should be called 3 times");
            Assert.AreEqual(0, weapon.ammo, "Ammo should remain 0");
        }

        [UnityTest]
        public IEnumerator Test18_DryFire_ReloadAfterDryFire()
        {
            // Arrange
            weapon.ammo = 0;
            weapon.TryFire(Vector3.zero, Vector3.forward); // Dry fire
            yield return null;

            // Act
            weapon.Reload();
            weapon.ResetFlags();
            bool fired = weapon.TryFire(Vector3.zero, Vector3.forward);
            yield return null;

            // Assert
            Assert.IsTrue(fired, "Should fire normally after reloading from dry fire");
            Assert.AreEqual(1, weapon.fireCallCount, "DoFire should be called after reload");
            Assert.AreEqual(0, weapon.dryFireCount, "OnDryFire should not be called after reload");
        }
        #endregion
    }
}
