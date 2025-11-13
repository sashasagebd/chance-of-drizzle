using UnityEngine;

/// <summary>
/// Automatically plays audio events when Health component events occur.
/// Attach this to any GameObject with a Health component.
/// Works with existing Health.OnDied event - no code changes needed.
/// </summary>
[RequireComponent(typeof(Health))]
public class HealthAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private bool playDeathSound = true;
    [SerializeField] private bool playLowHealthWarning = true;
    
    [Header("Low Health Threshold")]
    [SerializeField] [Range(0f, 0.5f)] private float lowHealthPercent = 0.25f;
    [SerializeField] private float lowHealthCooldown = 5f; // Prevent spam
    
    private Health _health;
    private int _lastHealth;
    private float _lastLowHealthWarning;
    private bool _hasWarnedLowHealth;
    
    void Awake()
    {
        _health = GetComponent<Health>();
        if (_health != null)
        {
            _lastHealth = _health.Current;
        }
    }
    
    void OnEnable()
    {
        if (_health != null)
        {
            _health.OnDied += OnDeath;
        }
    }
    
    void OnDisable()
    {
        if (_health != null)
        {
            _health.OnDied -= OnDeath;
        }
    }
    
    void Update()
    {
        if (_health == null) return;
        
        // Check for low health warning
        if (playLowHealthWarning)
        {
            float healthPercent = (float)_health.Current / _health.maxHp;
            if (healthPercent <= lowHealthPercent && !_hasWarnedLowHealth)
            {
                if (Time.time - _lastLowHealthWarning > lowHealthCooldown)
                {
                    SoundManager.Instance?.PlayPlayerLowHealth();
                    _lastLowHealthWarning = Time.time;
                    _hasWarnedLowHealth = true;
                }
            }
            else if (healthPercent > lowHealthPercent)
            {
                _hasWarnedLowHealth = false;
            }
        }
        
        _lastHealth = _health.Current;
    }
    
    void OnDeath()
    {
        if (playDeathSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPlayerDeath();
        }
    }
}

