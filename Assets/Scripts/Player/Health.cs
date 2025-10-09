using System;
using UnityEngine;

public class Health : MonoBehaviour {
    public int maxHp = 100;
    [SerializeField]  private bool destroyOnDeath = false;
    public int Current { get; private set; }
    public event Action OnDied;

    void Awake() { Current = maxHp; }

    public void ApplyDamage(int amount) {
        Current = Mathf.Max(0, Current - Mathf.Max(0, amount));
        Debug.Log($"{name} took {amount} damage. TOTAL HP: {Current}");
        if (Current <= 0)
        {
            if (destroyOnDeath) Destroy(gameObject); 
            OnDied?.Invoke();
        }
    }
}