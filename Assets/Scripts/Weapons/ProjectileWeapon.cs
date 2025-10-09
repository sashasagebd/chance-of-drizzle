using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    [Header("Projectile")]
    public GameObject bulletPrefab;      // assign your Bullet prefab
    public float muzzleSpeed = 60f;      // m/s
    public bool useGravity = false;      // toggle if you want drop
    public LayerMask bulletHitMask;      // same as bullet.hitMask

    protected override bool DoFire(Vector3 origin, Vector3 direction)
    {
        if (!bulletPrefab) return false;

        var go = Object.Instantiate(bulletPrefab, origin, Quaternion.LookRotation(direction));
        var bullet  = go.GetComponent<Bullet>();
        var rb = go.GetComponent<Rigidbody>();

        if (rb) rb.useGravity = useGravity;
        if (bullet)
        {
            bullet.damage  = damage;
            bullet.hitMask = bulletHitMask;
            bullet.Fire(direction.normalized * muzzleSpeed);
        }
        else
        {
            // fallback if no Bullet script
            if (rb) rb.linearVelocity = direction.normalized * muzzleSpeed;
        }

        return true;
    }
}