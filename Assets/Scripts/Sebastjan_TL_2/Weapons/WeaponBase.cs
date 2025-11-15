using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour {
    [Header("Stats")]
    public int magazineSize = 12;
    public float fireRate = 6f;
    public int damage = 10;

    [Header("Visual")]
    public Transform muzzle;  // Bullet spawn point for this weapon

    [Header("Runtime")]
    public int ammo;
    float _nextFireTime;
    private WeaponAudio weaponAudio;

    // notify HUD when ammo changes
    public event Action<int,int> OnAmmoChanged;

    protected virtual void Awake() {
        ammo = magazineSize;
        OnAmmoChanged?.Invoke(ammo, magazineSize);
    }

    public void Reload() {
        if (weaponAudio == null)
            weaponAudio = GetComponent<WeaponAudio>();
        weaponAudio?.OnWeaponReload(transform.position);

        ammo = magazineSize;
        OnAmmoChanged?.Invoke(ammo, magazineSize);
        OnReloaded();
    }

    /// <summary>
    /// Attempts to fire the weapon. Returns true if the weapon successfully fired.
    /// </summary>
    public bool TryFire(Vector3 origin, Vector3 direction) {
        if (Time.time < _nextFireTime) return false;
        if (ammo <= 0) { OnDryFire(); return false; }

        if (DoFire(origin, direction)) {
            ammo--;
            OnAmmoChanged?.Invoke(ammo, magazineSize);   // 🔔 update HUD
            _nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
            OnFired();
            return true;
        }
        return false;
    }

    protected abstract bool DoFire(Vector3 origin, Vector3 direction); // every child has to implement this function

    // subclass override to add an effect (sound, camera shake ...)
    protected virtual void OnFired() { }
    protected virtual void OnDryFire() { } // pressed fire but have no ammo
    protected virtual void OnReloaded() { }
}