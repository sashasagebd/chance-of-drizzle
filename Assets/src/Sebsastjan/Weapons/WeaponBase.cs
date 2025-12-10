using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Stats")]
    // PUBLIC: Configured via Unity Inspector for weapon balancing
    public int magazineSize = 12;
    public float fireRate = 6f;
    public int damage = 10;
    public int maxReserveAmmo = 120; // Maximum reserve ammunition
    public string weaponName = "Weapon"; // Display name for HUD
    public GameObject weaponIconPrefab; // 3D prefab or sprite to display in HUD

    [Header("Visual")]
    // PUBLIC: Accessed by PlayerController3D to determine bullet spawn position for firing
    public Transform muzzle; // Bullet spawn point for this weapon - PUBLIC: accessed by PlayerController3D
    [SerializeField] protected ParticleSystem muzzleFlash; // Muzzle flash particle effect
    [SerializeField] protected ParticleSystem hitEffect;
    [SerializeField] protected TrailRenderer tracerEffect;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private Transform raycastDestination;

    [Header("Runtime")]
    // PUBLIC: Accessed by UI (AmmoHUD) to display current ammo count
    public int ammo;
    public int reserveAmmo; // Current reserve ammunition
    float _nextFireTime;
    private WeaponAudio weaponAudio;

    // PUBLIC: Event subscribed by UI (AmmoHUD) to update ammo display when ammo changes
    // Parameters: current ammo in magazine, magazine size, reserve ammo
    public event Action<int, int, int> OnAmmoChanged;

    // Raycast visualization
    private Ray ray;
    private RaycastHit hitInfo;

    protected virtual void Awake()
    {
        ammo = magazineSize;
        reserveAmmo = maxReserveAmmo;
        OnAmmoChanged?.Invoke(ammo, magazineSize, reserveAmmo);
    }

    /// <summary>
    /// PUBLIC: Called by PlayerController3D when player presses reload input.
    /// Reloads the weapon from reserve ammo using CoD-style mechanics.
    /// </summary>
    public void Reload() {
        if (weaponAudio == null)
            weaponAudio = GetComponent<WeaponAudio>();
        weaponAudio?.OnWeaponReload(transform.position);

        // CoD-style reload: consume from reserve ammo
        int ammoNeeded = magazineSize - ammo; // How much we need to fill the magazine
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo); // Take from reserve, but not more than we have

        ammo += ammoToReload;
        reserveAmmo -= ammoToReload;

        OnAmmoChanged?.Invoke(ammo, magazineSize, reserveAmmo);
        OnReloaded();
    }

    /// <summary>
    /// PUBLIC: Called by PlayerController3D and AI (AIPlayer) to fire weapon
    /// Attempts to fire the weapon. Returns true if the weapon successfully fired.
    /// </summary>
    public bool TryFire(Vector3 origin, Vector3 direction)
    {
        // Skip rate limit when Time.time is 0 (EditMode tests)
        if (Time.time > 0 && Time.time < _nextFireTime) return false;
        if (ammo <= 0)
        {
            OnDryFire();
            return false;
        }

        if (DoFire(origin, direction))
        {
            ammo--;
            OnAmmoChanged?.Invoke(ammo, magazineSize, reserveAmmo); // 🔔 update HUD
            _nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
            OnFired();
            return true;
        }

        return false;
    }

    protected abstract bool DoFire(Vector3 origin, Vector3 direction); // every child has to implement this function

    // subclass override to add an effect (sound, camera shake ...)
    protected virtual void OnFired()
    {
        EmitMuzzleFlash();

        // Raycast visualization - only if components are set up
        // This allows weapons to work in tests without visual components
        if (raycastOrigin != null && raycastDestination != null && tracerEffect != null)
        {
            ray.origin = raycastOrigin.position;
            ray.direction = (raycastDestination.position - raycastOrigin.position).normalized;

            var tracer = Instantiate(tracerEffect, ray.origin, Quaternion.identity);
            tracer.AddPosition(ray.origin);

            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitEffect != null)
                {
                    hitEffect.transform.position = hitInfo.point;
                    hitEffect.transform.forward = hitInfo.normal;
                    hitEffect.Emit(1);
                }

                tracer.transform.position = hitInfo.point;
                // Debug.DrawLine(ray.origin, hitInfo.point, Color.red, 1.0f);
            }
        }
    }

    /// <summary>
    /// Emits a muzzle flash particle burst if a particle system is assigned.
    /// Emits one particle burst per shot fired.
    /// </summary>
    void EmitMuzzleFlash()
    {
        if (muzzleFlash)
        {
            try
            {
                muzzleFlash.Emit(1);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Muzzle flash emit failed: {e.Message}");
            }
        }
    }

    protected virtual void OnDryFire()
    {
    } // pressed fire but have no ammo

    protected virtual void OnReloaded()
    {
    }
}