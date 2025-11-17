using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Grenade : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;
    public float explosionRadius = 6.5f; // Medium radius (5-8 units)
    public LayerMask hitMask; // Layers that can be damaged

    [Header("Timing")]
    public float explosionTimer = 3f; // Time before explosion in seconds

    [Header("Visual Effects")]
    public ParticleSystem explosionEffect; // Explosion particle effect (can use hitEffect from weapon)
    public GameObject explosionPrefab; // Optional prefab to spawn on explosion

    private Rigidbody _rb;
    private float _spawnTime;
    private bool _hasExploded = false;
    private bool _hasFired = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 velocity)
    {
        _rb.linearVelocity = velocity;
        _spawnTime = Time.time;
        _hasFired = true;
    }

    /// <summary>
    /// Set the explosion effect to use (called from weapon)
    /// </summary>
    public void SetExplosionEffect(ParticleSystem effect)
    {
        explosionEffect = effect;
    }

    void Update()
    {
        // Check if it's time to explode (only if grenade has been fired)
        if (_hasFired && !_hasExploded && Time.time >= _spawnTime + explosionTimer)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (_hasExploded) return;
        _hasExploded = true;

        // If hitMask is not configured (0), use Everything except Ignore Raycast
        LayerMask explosionMask = hitMask;
        if (hitMask.value == 0)
        {
            Debug.LogWarning("Grenade hitMask not configured! Using all layers as fallback. Configure bulletHitMask on the weapon in Inspector.");
            explosionMask = ~0; // All layers
        }

        Debug.Log($"Grenade exploding at {transform.position}, radius: {explosionRadius}, damage: {damage}, hitMask: {explosionMask.value}");

        // Find all colliders in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, explosionMask);

        Debug.Log($"Grenade found {hitColliders.Length} colliders in explosion radius");

        foreach (var hitCollider in hitColliders)
        {
            Debug.Log($"Grenade checking collider: {hitCollider.gameObject.name} on layer {hitCollider.gameObject.layer}");

            // Calculate distance-based damage falloff
            float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
            float damageMultiplier = 1f - (distance / explosionRadius);
            int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

            Debug.Log($"Distance: {distance}, multiplier: {damageMultiplier}, final damage: {finalDamage}");

            // Try to apply damage to anything with Health component (player system)
            var health = hitCollider.GetComponent<Health>();
            if (health != null)
            {
                Debug.Log($"Applying {finalDamage} damage to Health component on {hitCollider.gameObject.name}");
                health.ApplyDamage(finalDamage);
            }

            // Try to apply damage to anything with EnemyController component (enemy system)
            var enemyController = hitCollider.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                Debug.Log($"Applying {finalDamage} damage to EnemyController on {hitCollider.gameObject.name}");
                enemyController.takeDamage(finalDamage);
            }

            if (health == null && enemyController == null)
            {
                Debug.LogWarning($"Grenade hit {hitCollider.gameObject.name} but found no Health or EnemyController component");
            }

            // Apply physics force to rigidbodies
            var rb = hitCollider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 explosionDirection = (hitCollider.transform.position - transform.position).normalized;
                float explosionForce = 500f;
                rb.AddForce(explosionDirection * explosionForce);
            }
        }

        // Spawn explosion visual effect
        if (explosionEffect != null)
        {
            // Instantiate a copy of the effect instead of modifying the original
            ParticleSystem effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            effect.transform.forward = Vector3.up; // Face upward
            effect.Play();
            Destroy(effect.gameObject, 5f); // Clean up after 5 seconds
        }

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 5f); // Clean up after 5 seconds
        }

        // Destroy the grenade
        Destroy(gameObject);
    }

    // Optional: Visualize explosion radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    // Explode on impact with anything in the hitMask
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Grenade collided with {collision.collider.gameObject.name} on layer {collision.collider.gameObject.layer}, hitMask: {hitMask.value}");

        // If hitMask is not configured, explode on any collision except terrain/ground
        if (hitMask.value == 0)
        {
            // Explode on everything except layer 9 (Ground) by default
            if (collision.collider.gameObject.layer != 9)
            {
                Debug.Log("HitMask not configured, exploding on non-ground collision");
                Explode();
            }
        }
        // Check if the collision is with something we can damage (in hitMask)
        else if (((1 << collision.collider.gameObject.layer) & hitMask) != 0)
        {
            Debug.Log("Collision layer matches hitMask, exploding!");
            Explode();
        }
        else
        {
            Debug.Log("Collision layer does NOT match hitMask");
        }
    }
}