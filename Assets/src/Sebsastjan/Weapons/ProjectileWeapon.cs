using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    [Header("Projectile")]
    // PUBLIC: Configured via Unity Inspector to specify which projectile prefab to spawn
    public GameObject bulletPrefab;      // assign your Bullet prefab
    // PUBLIC: Configured via Unity Inspector for projectile velocity tuning
    public float muzzleSpeed = 60f;      // m/s
    // PUBLIC: Configured via Unity Inspector to enable/disable physics gravity on projectiles
    public bool useGravity = false;      // toggle if you want drop
    [SerializeField] private LayerMask bulletHitMask;      // same as bullet.hitMask

    [Header("Grenade Launcher Mode")]
    [SerializeField] private bool isGrenadeLauncher = false; // Enable arc trajectory calculation
    [SerializeField] private float maxRange = 50f;         // Maximum range for grenade launcher
    [SerializeField] private float minLaunchAngle = 30f;   // Minimum launch angle in degrees
    [SerializeField] private float maxLaunchAngle = 75f;   // Maximum launch angle in degrees

    private WeaponAudio weaponAudio;
    private GameObject lastFiredProjectile; // Track last projectile for tracer
    private ProjectileTracer projectileTracer;

    protected override bool DoFire(Vector3 origin, Vector3 direction)
    {
        if (!bulletPrefab) return false;

        if (weaponAudio == null)
            weaponAudio = GetComponent<WeaponAudio>();
        weaponAudio?.OnWeaponFire(transform.position);

        // Use muzzle position if available, otherwise use origin
        Vector3 spawnPosition = muzzle != null ? muzzle.position : origin;

        Vector3 launchVelocity;
        Quaternion rotation;

        if (isGrenadeLauncher)
        {
            // Calculate arc trajectory to hit target point
            Vector3 targetPoint = CalculateTargetPoint(spawnPosition, direction);
            launchVelocity = CalculateArcVelocity(spawnPosition, targetPoint, out float angle);
            rotation = Quaternion.LookRotation(launchVelocity);

        }
        else
        {
            // Standard straight-line projectile
            launchVelocity = direction.normalized * muzzleSpeed;
            rotation = Quaternion.LookRotation(direction);
        }

        var go = Object.Instantiate(bulletPrefab, spawnPosition, rotation);
        go.SetActive(true); // Ensure bullet is active (in case prefab was inactive)
        lastFiredProjectile = go; // Store reference for tracer

        var bullet = go.GetComponent<Bullet>();
        var grenade = go.GetComponent<Grenade>();
        var rb = go.GetComponent<Rigidbody>();

        if (rb) rb.useGravity = useGravity || isGrenadeLauncher;

        if (grenade)
        {
            // Grenade launcher mode
            grenade.damage = damage + PlayerController3D.damageBonus;
            grenade.hitMask = bulletHitMask;

            // Pass the hitEffect as explosion effect
            if (hitEffect != null)
            {
                grenade.SetExplosionEffect(hitEffect);
            }

            grenade.Fire(launchVelocity);
        }
        else if (bullet)
        {
            // Standard bullet mode
            bullet.damage = damage + PlayerController3D.damageBonus;
            bullet.hitMask = bulletHitMask;
            bullet.Fire(launchVelocity);
        }
        else
        {
            // fallback if no Bullet or Grenade script
            if (rb) rb.linearVelocity = launchVelocity;
        }

        return true;
    }

    /// <summary>
    /// Calculate where the player is aiming using raycast
    /// </summary>
    Vector3 CalculateTargetPoint(Vector3 origin, Vector3 direction)
    {
        Ray aimRay = new Ray(origin, direction);

        // Try to hit something in the world
        if (Physics.Raycast(aimRay, out RaycastHit hit, maxRange))
        {
            return hit.point;
        }

        // If nothing hit, use max range
        return origin + direction.normalized * maxRange;
    }

    /// <summary>
    /// Calculate launch velocity for arc trajectory to hit target point
    /// Uses ballistic trajectory physics
    /// </summary>
    Vector3 CalculateArcVelocity(Vector3 start, Vector3 target, out float angle)
    {
        Vector3 toTarget = target - start;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
        float horizontalDistance = toTargetXZ.magnitude;
        float verticalDistance = toTarget.y;

        float gravity = Mathf.Abs(Physics.gravity.y);

        // Try to find a valid launch angle
        // We'll prefer a moderate angle for aesthetic arc
        float preferredAngle = 45f; // Start with 45 degrees

        // Calculate required velocity for preferred angle
        float angleRad = preferredAngle * Mathf.Deg2Rad;
        float velocity = CalculateVelocityForAngle(horizontalDistance, verticalDistance, gravity, angleRad);

        // If velocity is too high or invalid, try adjusting angle
        if (velocity > muzzleSpeed * 2f || velocity <= 0)
        {
            // Calculate angle for our fixed muzzle speed
            velocity = muzzleSpeed;
            angleRad = CalculateAngleForVelocity(horizontalDistance, verticalDistance, gravity, velocity);

            // Clamp angle to reasonable range
            angleRad = Mathf.Clamp(angleRad, minLaunchAngle * Mathf.Deg2Rad, maxLaunchAngle * Mathf.Deg2Rad);
        }
        else
        {
            velocity = Mathf.Min(velocity, muzzleSpeed * 1.5f);
        }

        angle = angleRad * Mathf.Rad2Deg;

        // Calculate velocity vector
        Vector3 horizontalDirection = toTargetXZ.normalized;
        Vector3 velocityXZ = horizontalDirection * velocity * Mathf.Cos(angleRad);
        Vector3 velocityY = Vector3.up * velocity * Mathf.Sin(angleRad);

        return velocityXZ + velocityY;
    }

    float CalculateVelocityForAngle(float distance, float heightDiff, float gravity, float angle)
    {
        float tanAngle = Mathf.Tan(angle);
        float cosAngle = Mathf.Cos(angle);

        float numerator = gravity * distance * distance;
        float denominator = 2 * cosAngle * cosAngle * (distance * tanAngle - heightDiff);

        if (denominator <= 0) return -1;

        return Mathf.Sqrt(numerator / denominator);
    }

    float CalculateAngleForVelocity(float distance, float heightDiff, float gravity, float velocity)
    {
        float v2 = velocity * velocity;
        float v4 = v2 * v2;
        float gd = gravity * distance;

        float underSqrt = v4 - gravity * (gravity * distance * distance + 2 * heightDiff * v2);

        if (underSqrt < 0) return 45f * Mathf.Deg2Rad; // Default to 45 degrees

        float angle1 = Mathf.Atan((v2 + Mathf.Sqrt(underSqrt)) / gd);
        float angle2 = Mathf.Atan((v2 - Mathf.Sqrt(underSqrt)) / gd);

        // Choose the lower angle for more direct shots
        return Mathf.Min(angle1, angle2);
    }

    // PUBLIC: Called by ProjectileTracer to track grenade trajectory for visual effects
    public GameObject GetLastFiredProjectile()
    {
        return lastFiredProjectile;
    }

    protected override void OnFired()
    {
        EmitMuzzleFlash();

        if (isGrenadeLauncher)
        {
            // For grenade launcher, create tracer that follows the projectile
            if (tracerEffect != null && lastFiredProjectile != null)
            {
                if (projectileTracer == null)
                {
                    projectileTracer = gameObject.AddComponent<ProjectileTracer>();
                }

                Vector3 startPos = muzzle != null ? muzzle.position : transform.position;
                projectileTracer.StartTracking(lastFiredProjectile, tracerEffect, startPos);
            }
            return;
        }

        // Standard raycast-based tracer for regular projectiles
        base.OnFired();
    }

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
}