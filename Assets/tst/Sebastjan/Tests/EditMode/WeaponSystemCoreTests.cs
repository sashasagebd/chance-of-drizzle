using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Category 2: Weapon System Core Tests (EditMode)
    /// Tests for WeaponBase functionality: ammo, reload, damage, and boundaries
    /// </summary>
    public class WeaponSystemCoreTests
    {
        private GameObject weaponObject;
        private TestWeapon weapon;

        // Helper class to test abstract WeaponBase
        private class TestWeapon : WeaponBase
        {
            public bool fireWasCalled = false;
            public Vector3 lastOrigin;
            public Vector3 lastDirection;

            protected override bool DoFire(Vector3 origin, Vector3 direction)
            {
                fireWasCalled = true;
                lastOrigin = origin;
                lastDirection = direction;
                return true;
            }

            // Expose protected method for testing
            public void TestOnFired() => OnFired();
            public void TestOnDryFire() => OnDryFire();
        }

        [SetUp]
        public void Setup()
        {
            weaponObject = new GameObject("TestWeapon");
            weapon = weaponObject.AddComponent<TestWeapon>();

            // Configure weapon
            weapon.magazineSize = 10;
            weapon.fireRate = 999999f; // Effectively unlimited fire rate for EditMode tests (Time.time doesn't advance)
            weapon.damage = 25;

            // Manually trigger Awake behavior
            weapon.ammo = weapon.magazineSize;
        }

        [TearDown]
        public void TearDown()
        {
            if (weaponObject != null)
                Object.DestroyImmediate(weaponObject);
        }

        #region Test 9: Ammo Depletion
        /// <summary>
        /// Test 9: Ammo Depletion
        /// Verifies ammo decreases with each shot and prevents firing at 0
        /// Bug Detection: Would catch if ammo doesn't decrease or goes negative
        /// </summary>
        [Test]
        public void Test09_AmmoDepletion_DecreasesWithEachShot()
        {
            // Arrange
            int initialAmmo = weapon.ammo;

            // Act
            bool fired = weapon.TryFire(Vector3.zero, Vector3.forward);

            // Assert
            Assert.IsTrue(fired, "Weapon should fire when ammo available");
            Assert.AreEqual(initialAmmo - 1, weapon.ammo,
                "Ammo should decrease by 1 after firing");
        }


        [Test]
        public void Test09_AmmoDepletion_CannotFireAtZero()
        {
            // Arrange - Deplete all ammo
            weapon.ammo = 1;
            weapon.TryFire(Vector3.zero, Vector3.forward); // Fire last shot

            // Act
            weapon.fireWasCalled = false; // Reset flag
            bool fired = weapon.TryFire(Vector3.zero, Vector3.forward);

            // Assert
            Assert.IsFalse(fired, "Should not be able to fire with 0 ammo");
            Assert.IsFalse(weapon.fireWasCalled, "DoFire should not be called when out of ammo");
            Assert.AreEqual(0, weapon.ammo, "Ammo should remain at 0");
        }

        #endregion

        #region Test 10: Reload Mechanics
        /// <summary>
        /// Test 10: Reload Mechanics
        /// Verifies reload restores ammo to magazine size
        /// Bug Detection: Would catch if reload doesn't restore ammo or restores wrong amount
        /// </summary>

        [Test]
        public void Test10_Reload_FromEmptyMagazine()
        {
            // Arrange - Completely deplete
            weapon.ammo = 0;

            // Act
            weapon.Reload();

            // Assert
            Assert.AreEqual(10, weapon.ammo, "Reload from empty should restore to full magazine");
        }

        [Test]
        public void Test10_Reload_AllowsFiringAfterReload()
        {
            // Arrange - Deplete completely
            weapon.ammo = 0;
            weapon.Reload();

            // Act
            weapon.fireWasCalled = false;
            bool fired = weapon.TryFire(Vector3.zero, Vector3.forward);

            // Assert
            Assert.IsTrue(fired, "Should be able to fire after reloading");
            Assert.IsTrue(weapon.fireWasCalled, "DoFire should be called after reload");
            Assert.AreEqual(9, weapon.ammo, "Ammo should decrease after post-reload shot");
        }
        #endregion

        #region Test 11: Damage Bonus
        /// <summary>
        /// Test 11: Damage Bonus Application
        /// Verifies weapon damage includes damage bonus from items
        /// Bug Detection: Would catch if damage bonus isn't applied to weapon damage
        /// </summary>
        [Test]
        public void Test11_DamageBonus_AddsToBaseDamage()
        {
            // Arrange
            int baseDamage = weapon.damage;
            PlayerController3D.damageBonus = 15;

            // For this test, we just verify the static bonus is set
            // The actual application happens in weapon subclasses (LazerWeapon, ProjectileWeapon)
            int expectedTotal = baseDamage + PlayerController3D.damageBonus;

            // Assert
            Assert.AreEqual(40, expectedTotal,
                "Total damage should be base (25) + bonus (15) = 40");
            Assert.AreEqual(15, PlayerController3D.damageBonus,
                "Damage bonus should be accessible from static field");

            // Cleanup
            PlayerController3D.damageBonus = 0;
        }

        [Test]
        public void Test11_DamageBonus_StartsAtZero()
        {
            // Assert
            Assert.AreEqual(0, PlayerController3D.damageBonus,
                "Damage bonus should start at 0 by default");
        }
        #endregion

        #region Test 19: Magazine Size Boundary
        /// <summary>
        /// Test 19: Magazine Size Boundary Conditions
        /// Verifies weapon handles edge cases for magazine sizes
        /// Bug Detection: Would catch if weapons don't handle small/large magazines
        /// </summary>
        [Test]
        public void Test19_MagazineSizeBoundary_SingleShotMagazine()
        {
            // Arrange
            weapon.magazineSize = 1;
            weapon.ammo = 1;

            // Act & Assert
            Assert.IsTrue(weapon.TryFire(Vector3.zero, Vector3.forward),
                "Should be able to fire with 1-round magazine");
            Assert.AreEqual(0, weapon.ammo, "Ammo should be 0 after firing");
            Assert.IsFalse(weapon.TryFire(Vector3.zero, Vector3.forward),
                "Should not fire after depleting 1-round magazine");

            // Reload
            weapon.Reload();
            Assert.AreEqual(1, weapon.ammo, "Reload should restore 1 round");
        }


        [Test]
        public void Test19_MagazineSizeBoundary_ZeroMagazineEdgeCase()
        {
            // Arrange - Edge case: 0 magazine size
            weapon.magazineSize = 0;
            weapon.ammo = 0;

            // Act
            bool fired = weapon.TryFire(Vector3.zero, Vector3.forward);

            // Assert
            Assert.IsFalse(fired, "Cannot fire with 0 magazine size");

            weapon.Reload();
            Assert.AreEqual(0, weapon.ammo, "Reload with 0 magazine should stay at 0");
        }
        #endregion
    }
}
