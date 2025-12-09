using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    // PUBLIC: Configured via Unity Inspector for gameplay balancing
    public float maxHp = 100f;
    [SerializeField] private bool destroyOnDeath = false;

    // PUBLIC: Read by UI systems (HealthHUD) and items to display/check current health
    public float Current { get; private set; }

    // PUBLIC: Event for other systems (ReloadOnDeath, MenuController) to respond to death
    public event Action OnDied;

    // PUBLIC: Event for UI systems (HealthHUD) to update health display when health changes
    public event Action<float, float> OnHealthChanged; // (current, max)
    private PlayerController3D playerController; // need for accessing armor modifier
    private bool _isDead = false;

    [SerializeField] private HealthHUD healthHUD;

    void Awake()
    {
        Current = maxHp;
        _isDead = false; // Reset death flag on awake
        playerController = GetComponent<PlayerController3D>();

        if (healthHUD != null)
            healthHUD.ApplyHealthChange(maxHp, maxHp);
    }

    // PUBLIC: Called by weapons (Bullet, Grenade, LazerWeapon) and hazards to damage entities
    public void ApplyDamage(float amount)
    {
        if (_isDead) return; // Don't apply damage if already dead

        float oldHp = Current;

        float defense = 0;
        if (playerController != null)
        {
            defense = playerController.currentDefense;
        }

        float percentDefense = Mathf.Clamp(defense, 0f, 1f); // clamp 0-1
        float damageTaken = amount * (1f - percentDefense); // Fixed: use percentDefense
        damageTaken = Mathf.Max(0f, damageTaken);

        Current = Mathf.Max(0f, Current - damageTaken);

        Debug.Log($"{name} took {amount} damage but armor defended {percentDefense * 100}% so only {damageTaken} damage taken. CURRENT HP: {Current}");

        // Update HUD
        if (healthHUD != null)
            healthHUD.ApplyHealthChange(oldHp, Current);

        OnHealthChanged?.Invoke(Current, maxHp);
        if (Current <= 0 && !_isDead)
        {
            _isDead = true; // Prevent multiple death events
            SoundManager.Instance?.PlayPlayerDeath(1f); // play death sound
            if (destroyOnDeath) Destroy(gameObject);
            OnDied?.Invoke();
        }

        if (!_isDead) // Only play damage sound if still alive
            SoundManager.Instance?.PlayPlayerDamage(.5f); // play hit sound
    }

    // PUBLIC: Called by items (Consumable) to restore player health
    public void Heal(int amount)
    {
        float oldHp = Current;

        Current = Mathf.Min(maxHp, Current + amount);

        // Update HUD
        if (healthHUD != null)
            healthHUD.ApplyHealthChange(oldHp, Current);

        OnHealthChanged?.Invoke(Current, maxHp);
    }

    // PUBLIC: Called by items to permanently increase max health capacity
    public void IncreaseMaxHealth(int amount)
    {
        maxHp += amount;
        Debug.Log($"Max health is now {maxHp}");

        if (healthHUD != null)
            healthHUD.ApplyHealthChange(Current, Current);
        OnHealthChanged?.Invoke(Current, maxHp);
    }

    // PUBLIC: Called by items and tests to directly set health value
    public void SetHealth(int value)
    {
        float oldHp = Current;
        Current = Mathf.Clamp(value, 1, maxHp);
        OnHealthChanged?.Invoke(Current, maxHp);

        if (healthHUD != null)
            healthHUD.ApplyHealthChange(oldHp, Current);
    }
}
