using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHp = 100f;
    [SerializeField] private bool destroyOnDeath = false;
    public float Current { get; private set; }
    public event Action OnDied;
    public event Action<float, float> OnHealthChanged; // (current, max)
    private PlayerController3D playerController; // need for accessing armor modifier

    public HealthHUD healthHUD;

    void Awake()
    {
        Current = maxHp;
        playerController = GetComponent<PlayerController3D>();

        if (healthHUD != null)
            healthHUD.ApplyHealthChange(maxHp, maxHp);
    }

    public void ApplyDamage(int amount)
    {
        float oldHp = Current;

        float defense = 0;
        if (playerController != null)
        {
            defense = playerController.currentDefense;
        }

        float percentDefense = Mathf.Clamp(defense, 0f, 1f); // clamp 0-1
        float damageTaken = amount * (1f - defense);
        damageTaken = Mathf.Max(0f, damageTaken);

        Current = Mathf.Max(0f, Current - damageTaken);

        Debug.Log($"{name} took {amount} damage but armor defended {percentDefense * 100}% so only {damageTaken} damage taken. CURRENT HP: {Current}");

        // Update HUD
        if (healthHUD != null)
            healthHUD.ApplyHealthChange(oldHp, Current);

        OnHealthChanged?.Invoke(Current, maxHp);
        if (Current <= 0)
        {
            if (destroyOnDeath) Destroy(gameObject);
            OnDied?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        float oldHp = Current;

        Current = Mathf.Min(maxHp, Current + amount);

        // Update HUD
        if (healthHUD != null)
            healthHUD.ApplyHealthChange(oldHp, Current);
        if (Current + amount <= maxHp)
        {
            Current += amount;
        }
        else
        {
            Current = maxHp;
        }
        OnHealthChanged?.Invoke(Current, maxHp);
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHp += amount;
        Debug.Log($"Max health is now {maxHp}");

        if (healthHUD != null)
            healthHUD.ApplyHealthChange(Current, Current);
        OnHealthChanged?.Invoke(Current, maxHp);
    }

    public void SetHealth(int value)
    {
        float oldHp = Current;
        Current = Mathf.Clamp(value, 1, maxHp);
        OnHealthChanged?.Invoke(Current, maxHp);

        if (healthHUD != null)
            healthHUD.ApplyHealthChange(oldHp, Current);
    }
}
