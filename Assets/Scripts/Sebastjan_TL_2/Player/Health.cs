using System;
using UnityEngine;

public class Health : MonoBehaviour {
    public float maxHp = 100f;
    [SerializeField]  private bool destroyOnDeath = false;
    public float Current { get; private set; }
    public event Action OnDied;
    private PlayerController3D playerController; // need for accessing armor modifier

    void Awake()
    {
        Current = maxHp;
        playerController = GetComponent<PlayerController3D>();
    }

    public void ApplyDamage(int amount)
    {
        float defense = 0;
        if (playerController != null)
        {
            defense = playerController.currentDefense;
        }

        float percentDefense = Mathf.Clamp(defense, 0f, 1f); // make sure between 0-1 for percent
        float damageTaken = amount * (1f - defense);
        damageTaken = Mathf.Max(0f, damageTaken);

        Current = Mathf.Max(0, Current - damageTaken);
        Debug.Log($"{name} took {amount} damage but armor defended {percentDefense * 100}% so only {damageTaken} damage taken. CURRENT HP: {Current}");
        if (Current <= 0)
        {
            if (destroyOnDeath) Destroy(gameObject);
            OnDied?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (Current + amount <= maxHp)
        {
            Current += amount;
        }
        else
        {
            Current = maxHp;
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHp += amount;
        Debug.Log($"Max health is now {maxHp}");
    }
    
    public void SetHealth(int value)
    {
        Current = Mathf.Clamp(value, 1, maxHp);
    }
}