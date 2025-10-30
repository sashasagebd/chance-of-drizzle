using System;
using UnityEngine;

public class Health : MonoBehaviour {
    public int maxHp = 100;
    [SerializeField]  private bool destroyOnDeath = false;
    public int Current { get; private set; }
    public event Action OnDied;
    private PlayerController3D playerController; // need for accessing armor modifier

    void Awake()
    {
        Current = maxHp;
        playerController = GetComponent<PlayerController3D>();
    }

    public void ApplyDamage(int amount)
    {
        int defense = 0;
        if (playerController != null)
        {
            defense = playerController.currentDefense;
        }

        int damageTaken = Mathf.Max(1, amount - defense);

        Current = Mathf.Max(0, Current - damageTaken);
        Debug.Log($"{name} took {amount} damage but armor defended {defense}. TOTAL HP: {Current}");
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