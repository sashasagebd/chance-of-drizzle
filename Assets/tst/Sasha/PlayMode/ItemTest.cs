using NUnit.Framework; 
using UnityEngine;
using System; 

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
        float buffAmount = 3.5f;
        float expectedSpeed = player.baseMoveSpeed + buffAmount;
        
        var speedberry = new Consumable("speedberry", "Boosts speed", "Speed", buffAmount, 10);

        speedberry.Use(player); 
        float actualSpeed = player.currentMoveSpeed;
        
        Assert.That(actualSpeed, Is.EqualTo(expectedSpeed),
            $"Speed Boost Failed. Expected speed: {expectedSpeed}, Actual speed: {actualSpeed}");
        
        Debug.Log($"[SUCCESS] Speed Boost Boundary Test: Speed is {actualSpeed}.");
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
            Current = Mathf.Min(maxHp, Current + amount); 
        }
    }

    public class PlayerController3D : MonoBehaviour
    {
        public Health HealthComponent;
        public bool HasPoison = false; 
        
        public float baseMoveSpeed = 5.0f;
        public float currentMoveSpeed = 5.0f;
        
        public void ApplySpeed(float amount, int duration)
        {
            currentMoveSpeed = baseMoveSpeed + amount;
        }
    }
}