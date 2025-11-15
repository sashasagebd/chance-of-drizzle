using UnityEngine;

/// <summary>
/// Standalone audio component for enemies. Attach to enemy GameObjects.
/// This component listens to enemy events without modifying Enemy.cs.
/// Note: Requires EnemyController or similar to expose events, or use UnityEvents in Inspector.
/// </summary>
public class EnemyAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private bool playAttackSound = true;
    [SerializeField] private bool playHitSound = true;
    [SerializeField] private bool playDeathSound = true;
    
    [Header("Attack Detection")]
    [SerializeField] private float attackSoundCooldown = 0.1f;
    
    private float _lastAttackTime;
    private EnemyController _enemyController;
    
    void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
    }
    
    // Public methods that can be called from other scripts or UnityEvents
    public void OnEnemyAttack(Vector3 position)
    {
        if (playAttackSound && Time.time - _lastAttackTime > attackSoundCooldown)
        {
            SoundManager.Instance?.PlayEnemyAttack(position);
            _lastAttackTime = Time.time;
        }
    }
    
    public void OnEnemyHit(Vector3 position)
    {
        if (playHitSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyHit(position);
        }
    }
    
    public void OnEnemyDeath(Vector3 position)
    {
        if (playDeathSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyDeath(position);
        }
    }
    
    public void OnEnemyLaser(Vector3 position)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyLaser(position);
        }
    }
    
    public void OnEnemyMissile(Vector3 position)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyMissile(position);
        }
    }
}

