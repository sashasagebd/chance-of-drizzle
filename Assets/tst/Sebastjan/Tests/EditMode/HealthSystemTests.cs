using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    /// Category 1: Health & Damage System Tests (EditMode)
    /// Tests for Health.cs basic functionality, healing, armor, and edge cases
    /// </summary>
    public class HealthSystemTests
    {
        private GameObject testObject;
        private Health health;
        private PlayerController3D playerController;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestPlayer");

            // Unity will automatically add CharacterController and PlayerInput
            // due to PlayerController3D's RequireComponent attribute
            playerController = testObject.AddComponent<PlayerController3D>();
            health = testObject.AddComponent<Health>();

            // Set max HP before Awake
            health.maxHp = 100f;

            // Trigger Awake manually using reflection
            var awakeMethod = typeof(Health).GetMethod("Awake",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            awakeMethod?.Invoke(health, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
                Object.DestroyImmediate(testObject);
        }

        #region Test 1: Basic Damage
        /// <summary>
        /// Test 1: Basic Damage Application
        /// Verifies that damage reduces health correctly without armor
        /// Bug Detection: Would catch if damage calculation is inverted or not applied
        /// </summary>
        [Test]
        public void Test01_BasicDamage_ReducesHealthCorrectly()
        {
            // Arrange
            float initialHealth = 100f;
            float damageAmount = 30f;
            float expectedHealth = 70f;

            // Act
            health.ApplyDamage(damageAmount);

            // Assert
            Assert.AreEqual(expectedHealth, health.Current, 0.01f,
                $"Health should be {expectedHealth} after taking {damageAmount} damage");
        }

        [Test]
        public void Test01_BasicDamage_MultipleDamageApplications()
        {
            // Arrange & Act
            health.ApplyDamage(25f);
            health.ApplyDamage(25f);
            health.ApplyDamage(25f);

            // Assert
            Assert.AreEqual(25f, health.Current, 0.01f,
                "Health should be 25 after three 25-damage hits");
        }

        [Test]
        public void Test01_BasicDamage_CannotGoNegative()
        {
            // Arrange & Act
            health.ApplyDamage(150f); // More than max health

            // Assert
            Assert.AreEqual(0f, health.Current, 0.01f,
                "Health should not go below 0");
        }
        #endregion

        #region Test 3: Healing
        /// <summary>
        /// Test 3: Healing Mechanics
        /// Verifies heal increases health and respects max health cap
        /// Bug Detection: Would catch the double-assignment bug in Heal() (now fixed)
        /// </summary>
        [Test]
        public void Test03_Healing_IncreasesHealthCorrectly()
        {
            // Arrange
            health.ApplyDamage(50f); // Reduce to 50 HP

            // Act
            health.Heal(30);

            // Assert
            Assert.AreEqual(80f, health.Current, 0.01f,
                "Healing 30 HP from 50 HP should result in 80 HP");
        }

        [Test]
        public void Test03_Healing_RespectsMaxHealthCap()
        {
            // Arrange
            health.ApplyDamage(20f); // Reduce to 80 HP

            // Act
            health.Heal(50); // Try to heal 50 (would exceed max)

            // Assert
            Assert.AreEqual(100f, health.Current, 0.01f,
                "Healing should not exceed max health (100)");
        }
        #endregion

        #region Test 4: Armor Defense
        /// <summary>
        /// Test 4: Armor Defense Calculation
        /// Verifies that armor reduces incoming damage correctly
        /// Bug Detection: Would catch if defense multiplier is wrong (was using unclamped defense)
        /// </summary>
        [Test]
        public void Test04_ArmorDefense_ReducesDamage()
        {
            // Arrange - 50% defense
            Armor chestplate = new Armor("Iron Chestplate", "Provides defense", "chestplate", 0.5f);
            playerController.EquipArmor(chestplate, out _);

            // Act
            health.ApplyDamage(100f);

            // Assert
            // With 50% defense, 100 damage becomes 50 damage
            // Expected: 100 - 50 = 50 HP remaining
            Assert.AreEqual(50f, health.Current, 0.01f,
                "50% armor should reduce 100 damage to 50 damage, leaving 50 HP");
        }

        [Test]
        public void Test04_ArmorDefense_MultipleHitsWithArmor()
        {
            // Arrange - 30% defense
            Armor helmet = new Armor("Iron Helmet", "Provides defense", "helmet", 0.3f);
            playerController.EquipArmor(helmet, out _);

            // Act - Take 3 hits of 40 damage each
            health.ApplyDamage(40f); // 40 * 0.7 = 28 damage -> 72 HP
            health.ApplyDamage(40f); // 28 damage -> 44 HP
            health.ApplyDamage(40f); // 28 damage -> 16 HP

            // Assert
            Assert.AreEqual(16f, health.Current, 0.01f,
                "30% armor should reduce each 40 damage to 28, leaving 16 HP after 3 hits");
        }
        #endregion

        #region Test 5: Multi-Slot Armor
        /// <summary>
        /// Test 5: Multiple Armor Pieces
        /// Verifies that multiple armor pieces stack defense correctly
        /// Bug Detection: Would catch if armor stacking doesn't work or defense isn't cumulative
        /// </summary>
        [Test]
        public void Test05_MultiSlotArmor_StacksDefense()
        {
            // Arrange - Equip 3 armor pieces
            Armor helmet = new Armor("Iron Helmet", "Head protection", "helmet", 0.1f);      // 10%
            Armor chestplate = new Armor("Iron Chestplate", "Chest protection", "chestplate", 0.2f); // 20%
            Armor boots = new Armor("Iron Boots", "Foot protection", "boots", 0.15f);        // 15%

            playerController.EquipArmor(helmet, out _);
            playerController.EquipArmor(chestplate, out _);
            playerController.EquipArmor(boots, out _);

            // Act
            health.ApplyDamage(100f);

            // Assert
            // Total defense: 0.1 + 0.2 + 0.15 = 0.45 (45%)
            // Damage taken: 100 * (1 - 0.45) = 55
            // Health: 100 - 55 = 45
            Assert.AreEqual(45f, health.Current, 0.01f,
                "45% total armor (10%+20%+15%) should reduce 100 damage to 55, leaving 45 HP");
        }

        [Test]
        public void Test05_MultiSlotArmor_IndependentSlots()
        {
            // Arrange - Equip different slot types
            Armor helmet = new Armor("Steel Helmet", "Head", "helmet", 0.2f);
            Armor gloves = new Armor("Steel Gloves", "Hands", "gloves", 0.1f);

            bool equipped1 = playerController.EquipArmor(helmet, out _);
            bool equipped2 = playerController.EquipArmor(gloves, out _);

            // Assert
            Assert.IsTrue(equipped1, "Should equip helmet");
            Assert.IsTrue(equipped2, "Should equip gloves in different slot");
            Assert.AreEqual(0.3f, playerController.currentDefense, 0.01f,
                "Total defense should be 30% (20% + 10%)");
        }
        #endregion

        #region Test 6: Armor Replacement
        /// <summary>
        /// Test 6: Armor Replacement Logic
        /// Verifies that better armor replaces worse armor, but not vice versa
        /// Bug Detection: Would catch if armor replacement logic allows downgrades
        /// </summary>
        [Test]
        public void Test06_ArmorReplacement_BetterArmorReplacesWorse()
        {
            // Arrange
            Armor weakHelmet = new Armor("Leather Helmet", "Weak", "helmet", 0.1f);
            Armor strongHelmet = new Armor("Diamond Helmet", "Strong", "helmet", 0.3f);

            playerController.EquipArmor(weakHelmet, out _);

            // Act
            bool replaced = playerController.EquipArmor(strongHelmet, out Armor oldArmor);

            // Assert
            Assert.IsTrue(replaced, "Better armor should replace worse armor");
            Assert.IsNotNull(oldArmor, "Should return the replaced armor");
            Assert.AreEqual("Leather Helmet", oldArmor.Name, "Should return the old helmet");
            Assert.AreEqual(0.3f, playerController.currentDefense, 0.01f,
                "Defense should update to new armor value");
        }

        [Test]
        public void Test06_ArmorReplacement_WorseArmorDoesNotReplace()
        {
            // Arrange
            Armor strongHelmet = new Armor("Diamond Helmet", "Strong", "helmet", 0.3f);
            Armor weakHelmet = new Armor("Leather Helmet", "Weak", "helmet", 0.1f);

            playerController.EquipArmor(strongHelmet, out _);

            // Act
            bool replaced = playerController.EquipArmor(weakHelmet, out Armor oldArmor);

            // Assert
            Assert.IsFalse(replaced, "Weaker armor should NOT replace stronger armor");
            Assert.AreEqual(0.3f, playerController.currentDefense, 0.01f,
                "Defense should remain at stronger armor value");
        }

        [Test]
        public void Test06_ArmorReplacement_EqualArmorDoesNotReplace()
        {
            // Arrange
            Armor helmet1 = new Armor("Iron Helmet A", "First", "helmet", 0.2f);
            Armor helmet2 = new Armor("Iron Helmet B", "Second", "helmet", 0.2f);

            playerController.EquipArmor(helmet1, out _);

            // Act
            bool replaced = playerController.EquipArmor(helmet2, out _);

            // Assert
            Assert.IsFalse(replaced, "Equal armor should NOT replace existing armor");
        }
        #endregion

        #region Test 7: Max Health Increase
        /// <summary>
        /// Test 7: Max Health Increase
        /// Verifies that max health can be increased and affects healing cap
        /// Bug Detection: Would catch if max health increase doesn't update properly
        /// </summary>
        [Test]
        public void Test07_MaxHealthIncrease_IncreasesMaximum()
        {
            // Arrange
            float originalMaxHp = health.maxHp;

            // Act
            health.IncreaseMaxHealth(50);

            // Assert
            Assert.AreEqual(150f, health.maxHp, 0.01f,
                "Max health should increase from 100 to 150");
        }

        [Test]
        public void Test07_MaxHealthIncrease_AllowsHealingToNewMax()
        {
            // Arrange
            health.ApplyDamage(50f); // Reduce to 50 HP
            health.IncreaseMaxHealth(50); // Max is now 150

            // Act
            health.Heal(100); // Heal to new max

            // Assert
            Assert.AreEqual(150f, health.Current, 0.01f,
                "Should be able to heal to new max health of 150");
        }
        #endregion

        #region Test 15: Overheal Prevention
        /// <summary>
        /// Test 15: Overheal Prevention
        /// Verifies healing cannot exceed max health
        /// Bug Detection: Would catch if healing allows health > maxHp
        /// </summary>
        [Test]
        public void Test15_OverhealPrevention_CannotExceedMaxHealth()
        {
            // Arrange
            health.ApplyDamage(10f); // 90 HP

            // Act
            health.Heal(50); // Try to overheal

            // Assert
            Assert.AreEqual(100f, health.Current, 0.01f,
                "Healing should cap at max health, not exceed it");
        }

        [Test]
        public void Test15_OverhealPrevention_HealingAtFullHealth()
        {
            // Act - Already at full health
            health.Heal(20);

            // Assert
            Assert.AreEqual(100f, health.Current, 0.01f,
                "Healing at full health should stay at max");
        }
        #endregion

        #region Test 16: Negative Damage
        /// <summary>
        /// Test 16: Negative Damage Handling
        /// Verifies that negative damage values don't heal
        /// Bug Detection: Would catch if negative damage acts as healing
        /// </summary>
        [Test]
        public void Test16_NegativeDamage_DoesNotHeal()
        {
            // Arrange
            health.ApplyDamage(50f); // Reduce to 50 HP
            float healthBefore = health.Current;

            // Act
            health.ApplyDamage(-20f); // Try negative damage

            // Assert
            // The implementation uses Mathf.Max(0, damageTaken) which prevents negative
            Assert.AreEqual(50f, health.Current, 0.01f,
                "Negative damage should not heal (should be clamped to 0)");
        }

        [Test]
        public void Test16_NegativeDamage_WithArmor()
        {
            // Arrange
            Armor armor = new Armor("Test Armor", "Test", "chestplate", 0.5f);
            playerController.EquipArmor(armor, out _);
            health.ApplyDamage(40f); // Reduce health

            // Act
            health.ApplyDamage(-10f);

            // Assert
            // 40 damage with 50% armor = 20 damage taken, leaving 80 HP
            // Then -10 damage should do nothing
            Assert.AreEqual(80f, health.Current, 0.01f,
                "Negative damage with armor should still not heal");
        }
        #endregion

        #region Test: Defense Clamping
        /// <summary>
        /// Test: Defense Clamping Prevents Negative Damage and Overheal
        /// Verifies that defense values are clamped to valid range [0, 1]
        /// and cannot cause negative damage (healing) or overhealing
        /// Bug Detection: Would catch if defense > 100% causes healing or health > maxHp
        /// </summary>
        [Test]
        public void Health_ApplyDamage_DefenseClamping_PreventsNegativeAndOverHeal()
        {
            // Arrange - Set health to half
            health.ApplyDamage(50f); // Health at 50 HP
            Assert.AreEqual(50f, health.Current, 0.01f, "Setup: Health should be at 50 HP");

            // Try to equip armor with >100% defense (should be clamped)
            // If defense isn't clamped, this could cause healing when taking damage
            Armor overDefenseArmor = new Armor("Invincible Armor", "Too strong", "chestplate", 1.5f); // 150% defense
            playerController.EquipArmor(overDefenseArmor, out _);

            // Act - Apply positive damage
            health.ApplyDamage(20f);

            // Assert - Health should not increase (defense shouldn't cause healing)
            Assert.LessOrEqual(health.Current, 50f,
                "Defense >100% should be clamped and not cause healing");
            Assert.GreaterOrEqual(health.Current, 0f,
                "Health should not go negative");

            // Additional check: Health should be at 50 (100% defense = 0 damage taken)
            // OR less if defense wasn't fully applied
            Assert.AreEqual(50f, health.Current, 0.01f,
                "With clamped 100% defense, 20 damage should be reduced to 0, health stays at 50");
        }

        [Test]
        public void Health_ApplyDamage_DefenseClamping_PreventsNegativeDefense()
        {
            // Arrange
            health.ApplyDamage(30f); // Health at 70 HP

            // Try to equip armor with negative defense (invalid)
            Armor negativeDefenseArmor = new Armor("Cursed Armor", "Increases damage?", "helmet", -0.5f);
            playerController.EquipArmor(negativeDefenseArmor, out _);

            // Act - Apply 10 damage
            health.ApplyDamage(10f);

            // Assert - Negative defense should be clamped to 0 (no bonus damage)
            // Expected: 70 - 10 = 60 HP (not 70 - 15 = 55 HP if negative defense amplified damage)
            Assert.GreaterOrEqual(health.Current, 60f,
                "Negative defense should be clamped to 0, not amplify damage");
            Assert.LessOrEqual(health.Current, 70f,
                "Negative defense should not heal");
        }

        [Test]
        public void Health_ApplyDamage_DefenseClamping_ExtremeDefenseStacking()
        {
            // Arrange - First reduce health WITHOUT armor
            health.ApplyDamage(50f); // Reduce to 50 HP
            Assert.AreEqual(50f, health.Current, 0.01f, "Setup: Health should be at 50 HP before equipping armor");

            // Now stack multiple high-defense armor pieces
            Armor armor1 = new Armor("Diamond Helmet", "Very strong", "helmet", 0.9f);      // 90%
            Armor armor2 = new Armor("Diamond Chestplate", "Very strong", "chestplate", 0.9f); // 90%
            Armor armor3 = new Armor("Diamond Boots", "Very strong", "boots", 0.9f);       // 90%
            // Total would be 270% if not clamped - should clamp to 100%

            playerController.EquipArmor(armor1, out _);
            playerController.EquipArmor(armor2, out _);
            playerController.EquipArmor(armor3, out _);

            // Act - Apply damage with extreme defense equipped
            health.ApplyDamage(30f);

            // Assert - Defense should be clamped to 100%, so 0 damage taken
            Assert.AreEqual(50f, health.Current, 0.01f,
                "Defense stack >100% should clamp to 100%, preventing all damage but not healing");
            Assert.LessOrEqual(health.Current, health.maxHp,
                "Health should never exceed max HP even with extreme defense");
        }

        [Test]
        public void Health_ApplyDamage_DefenseClamping_DoesNotAllowOverheal()
        {
            // Arrange - Already at full health
            Assert.AreEqual(100f, health.Current, 0.01f, "Setup: Should start at full health");

            // Equip extreme defense armor
            Armor extremeArmor = new Armor("God Armor", "Absurd", "chestplate", 999f); // 99900% defense
            playerController.EquipArmor(extremeArmor, out _);

            // Act - Take damage (which should be fully negated)
            health.ApplyDamage(50f);

            // Assert - Should still be at 100 HP, not above
            Assert.AreEqual(100f, health.Current, 0.01f,
                "Extreme defense with full health should not cause overheal");
            Assert.LessOrEqual(health.Current, health.maxHp,
                "Health must not exceed max HP under any circumstances");
        }
        #endregion
    }
}
