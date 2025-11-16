using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Stats")] public int magazineSize = 12;
    public float fireRate = 6f;
    public int damage = 10;

    [Header("Visual")] public Transform muzzle; // Bullet spawn point for this weapon
    public ParticleSystem muzzleFlash; // Muzzle flash particle effect
    public ParticleSystem hitEffect;
    public TrailRenderer tracerEffect;
    public Transform raycastOrigin;
    public Transform raycastDestination;

    [Header("Runtime")] public int ammo;
    float _nextFireTime;

    // notify HUD when ammo changes
    public event Action<int, int> OnAmmoChanged;

    // Raycast visualization
    private Ray ray;
    private RaycastHit hitInfo;

    protected virtual void Awake()
    {
        ammo = magazineSize;
        OnAmmoChanged?.Invoke(ammo, magazineSize);
    }

    public void Reload()
    {
        ammo = magazineSize;
        OnAmmoChanged?.Invoke(ammo, magazineSize);
        OnReloaded();
    }

    /// <summary>
    /// Attempts to fire the weapon. Returns true if the weapon successfully fired.
    /// </summary>
    public bool TryFire(Vector3 origin, Vector3 direction)
    {
        if (Time.time < _nextFireTime) return false;
        if (ammo <= 0)
        {
            OnDryFire();
            return false;
        }

        if (DoFire(origin, direction))
        {
            ammo--;
            OnAmmoChanged?.Invoke(ammo, magazineSize); // 🔔 update HUD
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

        // Raycast visualization
        ray.origin = raycastOrigin.position;
        ray.direction = (raycastDestination.position - raycastOrigin.position).normalized;
        
        var tracer = Instantiate(tracerEffect, ray.origin, Quaternion.identity);
        tracer.AddPosition(ray.origin);

        if (Physics.Raycast(ray, out hitInfo))
        {
            hitEffect.transform.position = hitInfo.point;
            hitEffect.transform.forward = hitInfo.normal;
            hitEffect.Emit(1);
            
            
            tracer.transform.position = hitInfo.point;
            // Debug.DrawLine(ray.origin, hitInfo.point, Color.red, 1.0f);
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