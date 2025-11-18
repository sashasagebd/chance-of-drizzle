using UnityEngine;

/// <summary>
/// Manages a tracer effect that follows a projectile
/// Attach this to the weapon, not the projectile
/// </summary>
public class ProjectileTracer : MonoBehaviour
{
    private TrailRenderer activeTracer;
    private GameObject trackedProjectile;

    public void StartTracking(GameObject projectile, TrailRenderer tracerPrefab, Vector3 startPosition)
    {
        if (tracerPrefab == null) return;

        // Instantiate tracer at start position
        activeTracer = Instantiate(tracerPrefab, startPosition, Quaternion.identity);
        activeTracer.AddPosition(startPosition);

        trackedProjectile = projectile;
    }

    void LateUpdate()
    {
        if (activeTracer != null && trackedProjectile != null)
        {
            // Update tracer position to follow projectile
            activeTracer.transform.position = trackedProjectile.transform.position;
        }
        else if (activeTracer != null && trackedProjectile == null)
        {
            // Projectile destroyed, clean up tracer after a delay
            Destroy(activeTracer.gameObject, activeTracer.time);
            activeTracer = null;
        }
    }
}