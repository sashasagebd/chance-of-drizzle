using System;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour {
    [Header("Stats")]
    public int magazineSize = 12;
    public float fireRate = 6f;
    public int damage = 10;

    [Header("Runtime")]
    public int ammo;
    float _nextFireTime;

    // notify HUD when ammo changes
    public event Action<int,int> OnAmmoChanged;

    protected virtual void Awake() {
        ammo = magazineSize;
        OnAmmoChanged?.Invoke(ammo, magazineSize);
    }

    public void Reload() {
        ammo = magazineSize;
        OnAmmoChanged?.Invoke(ammo, magazineSize);
        OnReloaded();
    }

    public void TryFire(Vector3 origin, Vector3 direction) {
        if (Time.time < _nextFireTime) return;
        if (ammo <= 0) { OnDryFire(); return; }

        if (DoFire(origin, direction)) {
            ammo--;
            OnAmmoChanged?.Invoke(ammo, magazineSize);   // 🔔 update HUD
            _nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
            OnFired();
        }
    }

    protected abstract bool DoFire(Vector3 origin, Vector3 direction); // every child has to implement this function

    // subclass override to add an effect (sound, camera shake ...)
    protected virtual void OnFired() { }
    protected virtual void OnDryFire() { } // pressed fire but have no ammo
    protected virtual void OnReloaded() { }
}