using UnityEngine;

/// <summary>
/// Plays audio for weapon events. 
/// 
/// USAGE: Create custom weapon classes that extend ProjectileWeapon/LazerWeapon
/// and call these methods from OnFired(), OnDryFire(), OnReloaded().
/// 
/// Example:
/// public class MyWeapon : ProjectileWeapon {
///     private WeaponAudio audio;
///     void Awake() { audio = GetComponent<WeaponAudio>(); }
///     protected override void OnFired() {
///         base.OnFired();
///         audio?.OnWeaponFire(transform.position);
///     }
/// }
/// </summary>
public class WeaponAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private bool playFireSound = true;
    [SerializeField] private bool playReloadSound = true;
    [SerializeField] private bool playDryFireSound = true;
    
    // Public methods that can be called from custom weapon classes
    public void OnWeaponFire(Vector3 position)
    {
        if (playFireSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWeaponFire(position);
        }
    }
    
    public void OnWeaponReload(Vector3 position)
    {
        if (playReloadSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWeaponReload(position);
        }
    }
    
    public void OnWeaponDryFire(Vector3 position)
    {
        if (playDryFireSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWeaponDryFire(position);
        }
    }
}

