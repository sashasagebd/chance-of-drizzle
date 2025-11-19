using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    /// <summary>
    /// Category 1: Health & Damage System - Death Event Test (PlayMode)
    /// Tests that require play mode for event testing and coroutines
    /// </summary>
    public class HealthDeathEventTests
    {
        private GameObject testObject;
        private Health health;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            testObject = new GameObject("TestPlayer");
            health = testObject.AddComponent<Health>();
            health.maxHp = 100f;

            yield return null; // Wait for Awake to trigger
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (testObject != null)
                Object.Destroy(testObject);

            yield return null;
        }

        #region Test 2: Death Event
        /// <summary>
        /// Test 2: Death Event Triggering
        /// Verifies OnDied event fires when health reaches 0
        /// Bug Detection: Would catch if death event never fires or fires multiple times
        /// </summary>
        [UnityTest]
        public IEnumerator Test02_DeathEvent_TriggersOnZeroHealth()
        {
            // Arrange
            bool deathEventFired = false;
            health.OnDied += () => deathEventFired = true;

            // Act
            health.ApplyDamage(100f); // Lethal damage
            yield return null;

            // Assert
            Assert.IsTrue(deathEventFired, "OnDied event should fire when health reaches 0");
            Assert.AreEqual(0f, health.Current, 0.01f, "Health should be 0 after lethal damage");
        }

        [UnityTest]
        public IEnumerator Test02_DeathEvent_FiresOnlyOnce()
        {
            // Arrange
            int deathEventCount = 0;
            health.OnDied += () => deathEventCount++;

            // Act - Apply lethal damage multiple times
            health.ApplyDamage(60f);
            yield return null;
            health.ApplyDamage(50f);
            yield return null;
            health.ApplyDamage(100f); // Additional damage after death
            yield return null;

            // Assert
            Assert.AreEqual(1, deathEventCount,
                "OnDied event should fire exactly once, even with multiple damage after death");
        }

        [UnityTest]
        public IEnumerator Test02_DeathEvent_NoEventWhenNotDead()
        {
            // Arrange
            bool deathEventFired = false;
            health.OnDied += () => deathEventFired = true;

            // Act - Non-lethal damage
            health.ApplyDamage(50f);
            yield return null;
            health.ApplyDamage(30f);
            yield return null;

            // Assert
            Assert.IsFalse(deathEventFired, "OnDied should NOT fire when health is above 0");
            Assert.Greater(health.Current, 0f, "Health should still be above 0");
        }

        [UnityTest]
        public IEnumerator Test02_DeathEvent_ExactlyZeroDamage()
        {
            // Arrange
            bool deathEventFired = false;
            health.OnDied += () => deathEventFired = true;

            // Act - Exactly 100 damage (exact lethal)
            health.ApplyDamage(100f);
            yield return null;

            // Assert
            Assert.IsTrue(deathEventFired, "OnDied should fire when damage equals exactly max health");
            Assert.AreEqual(0f, health.Current, 0.01f);
        }
        #endregion
    }
}
