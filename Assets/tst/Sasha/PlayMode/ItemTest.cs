using NUnit.Framework; 
using UnityEngine;
using System; 
using System.Collections;

[TestFixture]
public class ItemTestRunner
{
    private PlayerController3D player; //mock classes since assembly linking wasn't working
    private Health health;

    [SetUp]
    public void Setup()
    {
        GameObject go = new GameObject("TEST_ENTITY");
        player = go.AddComponent<PlayerController3D>();
        health = go.AddComponent<Health>();
        player.HealthComponent = health;
        
        health.maxHp = 100;
        health.SetHealth(health.maxHp); 
    }
    
    [TearDown]
    public void Teardown()
    {
        if (player != null && player.gameObject != null)
        {
            UnityEngine.Object.DestroyImmediate(player.gameObject);
        }
    }

    [Test]
    public void HealClampBoundaryTest()
    {
        health.SetHealth(health.maxHp - 1); 
        
        var potion = new Consumable("healpotion", "Heals for 20 health", "Heal", 20, 0); 
        int expectedHP = (int)health.maxHp; 

        potion.Use(player); 
        int actualFinalHP = (int)health.Current;

        Assert.That(actualFinalHP, Is.EqualTo(expectedHP), 
            $"Heal Clamp Failed. Expected HP: {expectedHP}, Actual HP: {actualFinalHP}");
        
        Debug.Log($"[SUCCESS] Heal Clamp Boundary Test: HP clamped correctly at Max ({actualFinalHP}).");
    }

    [Test]
    public void SpeedBoostBoundaryTest()
    {
        float buffAmount = 2f;
        float expectedRunSpeed = player.runSpeed + buffAmount;
        float expectedSprintSpeed = player.sprintSpeed + buffAmount;
        
        var speedberry = new Consumable("speedberry", "Boosts speed", "Speed", buffAmount, 10);

        speedberry.Use(player); 
        float actualRunSpeed = player.runSpeed;
        float actualSprintSpeed = player.sprintSpeed;
        
        Assert.That(actualRunSpeed, Is.EqualTo(expectedRunSpeed),
            $"Speed Boost Failed. Expected run speed: {expectedRunSpeed}, Actual run speed: {actualRunSpeed}");
        Assert.That(actualSprintSpeed, Is.EqualTo(expectedSprintSpeed),
            $"Speed Boost Failed. Expected sprint speed: {expectedSprintSpeed}, Actual sprint speed: {actualSprintSpeed}");
        
        Debug.Log($"[SUCCESS] Speed Boost Boundary Test: Speed is {actualRunSpeed} (run), {actualSprintSpeed} (sprint).");
    }


    [Test]
    public void HealSpamStressTest()
    {
        var potion = new Consumable("healpotion", "Heals for 20 health", "Heal", 20, 0); 

        int spamCount = 10000;
        float expectedHP = health.maxHp;

        for (int i = 0; i < spamCount; i++)
        {
            potion.Use(player);
        }
        float actualFinalHP = health.Current;

        Assert.That(actualFinalHP, Is.EqualTo(expectedHP),
            $"Heal Spam Failed. Expected HP: {expectedHP}, Actual HP: {actualFinalHP}");

        Debug.Log($"[SUCCESS] Heal Spam Stress Test: HP correctly remained at {actualFinalHP} after {spamCount} heals.");
    }

    [Test]
    public void HealDoesNotExceedMaxHPTest()
    {
        health.SetHealth(95);
        var potion = new Consumable("healpotion", "Heals", "Heal", 20, 0);

        potion.Use(player);

        Assert.That(health.Current, Is.EqualTo(100));
    }

    [Test]
    public void HealFromOneTest()
    {
        health.SetHealth(1);
        var potion = new Consumable("healpotion", "", "Heal", 20, 0);

        potion.Use(player);

        Assert.That(health.Current, Is.EqualTo(21));
    }

    [Test]
    public void DamageClampAtZeroTest()
    {
        health.SetHealth(5);
        health.ApplyDamage(999);

        Assert.That(health.Current, Is.EqualTo(0));
    }

    [Test]
    public void SpeedBuffStackingTest()
    {
        var berry = new Consumable("speedberry", "", "Speed", 1f, 0);

        berry.Use(player);
        berry.Use(player);

        Assert.That(player.runSpeed, Is.EqualTo(6f)); 
        Assert.That(player.sprintSpeed, Is.EqualTo(11f));
    }
    [Test]
    public void SpeedBoostRevertsTest()
    {
        var berry = new Consumable("speedberry", "", "Speed", 1f, 1);

        berry.Use(player);
        Assert.That(player.runSpeed, Is.EqualTo(5f));

        player.StartCoroutine(WaitAndExecute(1.1f, () =>
        {
            Assert.That(player.runSpeed, Is.EqualTo(4f));
        }));
    }

    [Test]
    public void SpeedBuffNoRevertTest()
    {
        var berry = new Consumable("item", "", "Speed", 2f, 0);

        berry.Use(player);

        Assert.That(player.runSpeed, Is.EqualTo(6f));
        
        player.StartCoroutine(WaitAndExecute(1f, () =>
        {
            Assert.That(player.runSpeed, Is.EqualTo(6f));
        }));
    }

    [Test]
    public void JumpBuffChangesJumpSpeedTest()
    {
        float original = player.jumpSpeed;
        var item = new Consumable("item", "", "Jump", 3f, 0);

        item.Use(player);

        Assert.That(player.jumpSpeed, Is.EqualTo(original + 3f));
    }

    [Test]
    public void JumpBoostRevertsTest()
    {
        var item = new Consumable("item", "", "Jump", 3f, 1);

        item.Use(player);
        Assert.That(player.jumpSpeed, Is.EqualTo(8.5f));

        player.StartCoroutine(WaitAndExecute(1.1f, () =>
        {
            Assert.That(player.jumpSpeed, Is.EqualTo(5.5f));
        }));
    }

    [Test]
    public void HealItemDoesNothingWithoutHealthComponentTest()
    {
        player.HealthComponent = null; 
        health.SetHealth(50);
        
        var potion = new Consumable("item", "", "Heal", 20, 0);
        potion.Use(player);

        Assert.That(health.Current, Is.EqualTo(50));
    }

    [Test]
    public void UsingItemOnInvalidTargetDoesNothingTest()
    {
        var potion = new Consumable("item", "", "Heal", 20, 0);

        int initial = (int)health.Current;
        potion.Use(new object());

        Assert.That((int)health.Current, Is.EqualTo(initial));
    }

    [Test]
    public void NegativeHealDoesNotReduceHPTest()
    {
        var potion = new Consumable("item", "", "Heal", -20, 0);

        potion.Use(player);

        Assert.That(health.Current, Is.EqualTo(100));
    }

    [Test]
    public void MultipleDifferentBuffsApplyCorrectlyTest()
    {
        var heal = new Consumable("item", "", "Heal", 20, 0);
        var jump = new Consumable("item", "", "Jump", 3f, 0);

        health.SetHealth(50);
        heal.Use(player);
        jump.Use(player);

        Assert.That(health.Current, Is.EqualTo(70));
        Assert.That(player.jumpSpeed, Is.EqualTo(8.5f));
    }

    [Test]
    public void HealDoesNothingWhenAmountIsZeroTest()
    {
        health.SetHealth(50);
        var potion = new Consumable("item", "", "Heal", 0, 0);

        potion.Use(player);

        Assert.That(health.Current, Is.EqualTo(50));
    }

    [Test]
    public void SpeedBoostStopsPreviousCoroutineTest()
    {
        var berry = new Consumable("item", "", "Speed", 1f, 5);

        berry.Use(player);  
        var oldSpeed = player.runSpeed;

        berry.Use(player); 

        Assert.That(player.runSpeed, Is.EqualTo(oldSpeed + 1f));
    }

    [Test]
    public void JumpBoostStopsPreviousCoroutineTest()
    {
        var fruit = new Consumable("item", "", "Jump", 2f, 5);

        fruit.Use(player); 
        var oldJump = player.jumpSpeed;

        fruit.Use(player);          

        Assert.That(player.jumpSpeed, Is.EqualTo(oldJump + 2f));
    }

    [Test]
    public void JumpBoostWithZeroAmountTest()
    {
        float before = player.jumpSpeed;
        var item = new Consumable("item", "", "Jump", 0, 5);

        item.Use(player);

        Assert.That(player.jumpSpeed, Is.EqualTo(before));
    }

    [Test]
    public void MultipleSpeedItemsStackCorrectlyTest()
    {
        var b1 = new Consumable("b1", "", "Speed", 1f, 0);
        var b2 = new Consumable("b2", "", "Speed", 3f, 0);

        b1.Use(player);
        b2.Use(player);

        Assert.That(player.runSpeed, Is.EqualTo(8f));
    }

    [Test]
    public void HealDoesNotAffectSpeedTest()
    {
        float r = player.runSpeed;
        float s = player.sprintSpeed;

        var heal = new Consumable("item", "", "Heal", 50, 0);
        heal.Use(player);

        Assert.That(player.runSpeed, Is.EqualTo(r));
        Assert.That(player.sprintSpeed, Is.EqualTo(s));
    }

    [Test]
    public void SpeedBuffDuringJumpBuffTest()
    {
        float originalJump = player.jumpSpeed;

        var jump = new Consumable("item", "", "Jump", 2f, 0);
        var speed = new Consumable("item", "", "Speed", 2f, 0);

        jump.Use(player);
        speed.Use(player);

        Assert.That(player.jumpSpeed, Is.EqualTo(originalJump + 2f));
    }

    [Test]
    public void JumpBuffDuringSpeedBuffTest()
    {
        float originalRun = player.runSpeed;

        var speed = new Consumable("item", "", "Speed", 3f, 0);
        var jump = new Consumable("item", "", "Jump", 4f, 0);

        speed.Use(player);
        jump.Use(player);

        Assert.That(player.runSpeed, Is.EqualTo(originalRun + 3f));
    }

    [Test]
    public void ConsumableStoresDataTest()
    {
        var c = new Consumable("Apple", "Yummy", "Heal", 5, 10);

        Assert.That(c.Name, Is.EqualTo("Apple"));
        Assert.That(c.Description, Is.EqualTo("Yummy"));
        Assert.That(c.EffectType, Is.EqualTo("Heal"));
        Assert.That(c.Amount, Is.EqualTo(5));
        Assert.That(c.Duration, Is.EqualTo(10));
    }

    [Test]
    public void DamageThenHealTest()
    {
        health.SetHealth(20);
        health.ApplyDamage(10);

        var potion = new Consumable("item", "", "Heal", 50, 0);
        potion.Use(player);

        Assert.That(health.Current, Is.EqualTo(60));
    }

    [Test]
    public void SpeedBuffThenDamageTest()
    {
        var berry = new Consumable("item", "", "Speed", 2f, 0);
        berry.Use(player);

        float expected = player.runSpeed;

        health.ApplyDamage(999);

        Assert.That(player.runSpeed, Is.EqualTo(expected));
    }

    [Test]
    public void JumpBuffThenDamageTest()
    {
        var fruit = new Consumable("item", "", "Jump", 2f, 0);
        fruit.Use(player);

        float expected = player.jumpSpeed;

        health.ApplyDamage(999);

        Assert.That(player.jumpSpeed, Is.EqualTo(expected));
    }

    [Test]
    public void ApplySpeedTwiceWithDifferentDurationsTest()
    {
        var b1 = new Consumable("item", "", "Speed", 2f, 1);
        var b2 = new Consumable("item", "", "Speed", 2f, 5);

        b1.Use(player);
        b2.Use(player);

        Assert.That(player.runSpeed, Is.EqualTo(8f)); 
    }

    [Test]
    public void SpeedBuffExpiresWhileJumpBuffStaysTest()
    {
        var speed = new Consumable("item", "", "Speed", 2f, 1);
        var jump  = new Consumable("item", "", "Jump", 5f, 0);

        float originalJump = player.jumpSpeed;

        speed.Use(player);
        jump.Use(player);

        player.StartCoroutine(WaitAndExecute(1.1f, () =>
        {
            Assert.That(player.runSpeed, Is.EqualTo(4f)); // speed reset
            Assert.That(player.jumpSpeed, Is.EqualTo(originalJump + 5f)); // jump stays
        }));
    }

    [Test]
    public void UsingItemOnNullDoesNotErrorTest()
    {
        var potion = new Consumable("item", "", "Heal", 20, 0);

        Assert.DoesNotThrow(() =>
        {
            potion.Use(null);
        });
    }



    private IEnumerator WaitAndExecute(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback?.Invoke();
    }
    
    public abstract class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CanStack { get; set; }

        public abstract void Use(object target);
    }

    public class Consumable : Item
    {
        public int Duration { get; set; }
        public string EffectType { get; set; }
        public float Amount { get; set; }

        public Consumable(string name, string description, string effect, float amount, int duration)
        {
            Name = name;
            Description = description;
            EffectType = effect;
            Amount = amount;
            Duration = duration;
            CanStack = true;
        }


        public override void Use(object target)
        {
            if (target is PlayerController3D player)
            {
                if (EffectType == "Speed")
                {
                    player.ApplySpeed(Amount, Duration);
                }
                else if (EffectType == "Heal")
                {
                    if (player.HealthComponent != null)
                    {
                        player.HealthComponent.Heal((int)Amount);
                    }
                }
                else if (EffectType == "Jump")
                {
                    player.ApplyJumpBoost(Amount, Duration);
                }
            }
        }
    }

    public class Health : MonoBehaviour
    {
        public float maxHp = 100;
        public float Current { get; private set; } = 100; 

        public void SetHealth(float value) => Current = value;
        public void ApplyDamage(float damage) 
        {
            Current = Mathf.Max(0, Current - damage); 
        }
        public void Heal(int amount) 
        {
            if(amount < 0) return;
            Current = Mathf.Min(maxHp, Current + amount); 
        }
    }

    public class PlayerController3D : MonoBehaviour
    {
        public Health HealthComponent; // needed right now for items to access health class easily
        private Coroutine speedTimer; // time for temp speed buffs
        private Coroutine jumpTimer; // time for temp jump buffs
        public float runSpeed = 4f;      
        public float sprintSpeed = 9f;    
        public float crouchSpeed = 2f;    
        public float jumpSpeed = 5.5f;

        
        public void ApplySpeed(float amount, int duration)
        {
            if (speedTimer != null)
            {
                StopCoroutine(speedTimer);
                speedTimer = null;
            }

            // Apply speed buff to all movement speeds
            runSpeed += amount;
            sprintSpeed += amount;
            Debug.Log($"Speed was increased by {amount} for a total run speed of {runSpeed}");

            if (duration > 0) // If duration is 0 then permanent speed buff
            {
                speedTimer = StartCoroutine(SpeedBuffCoroutine(amount, duration));
            }
        }

        private IEnumerator SpeedBuffCoroutine(float amount, int duration)
        {
            // Wait for the duration time
            yield return new WaitForSeconds(duration);

            // Remove speed buff from all movement speeds
            runSpeed -= amount;
            sprintSpeed -= amount;

            Debug.Log($"Temporary speed boost expired. Total run speed reset to {runSpeed}.");

            speedTimer = null;
        }

        public void ApplyJumpBoost(float amount, int duration)
        {
            if (jumpTimer != null)
            {
                StopCoroutine(jumpTimer);
                jumpTimer = null;
            }
            
            jumpSpeed += amount;
            Debug.Log($"Jumping speed was increased by {amount} for a total speed of {jumpSpeed}");

            if (duration > 0) // If duration is 0 then permanent jump buff
            {
                jumpTimer = StartCoroutine(JumpBuffCoroutine(amount, duration));
            }
        }

        private IEnumerator JumpBuffCoroutine(float amount, int duration)
        {
            // Wait for the duration time
            yield return new WaitForSeconds(duration);

            jumpSpeed -= amount;

            Debug.Log($"Temporary jump speed boost expired. Total jump speed reset to {jumpSpeed}.");

            jumpTimer = null;
        }
    }
}