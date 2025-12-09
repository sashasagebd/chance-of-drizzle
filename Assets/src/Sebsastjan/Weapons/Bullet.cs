using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    // PUBLIC: Set by weapon (ProjectileWeapon) when bullet is spawned, includes damage bonuses
    public int damage = 10;
    // PUBLIC: Configured via Unity Inspector to control how long bullet exists before despawning
    public float lifetime = 3f;
    // PUBLIC: Set by weapon (ProjectileWeapon) to specify which layers the bullet can damage
    public LayerMask hitMask; // include Hittable, exclude Weapon & Projectile
    Rigidbody _rb;

    void Awake() => _rb = GetComponent<Rigidbody>();

    // PUBLIC: Called by weapon (ProjectileWeapon) to launch bullet with specified velocity
    public void Fire(Vector3 velocity)
    {
        // Ensure Rigidbody is initialized (in case Fire is called before Awake)
        if (_rb == null) _rb = GetComponent<Rigidbody>();

        _rb.linearVelocity = velocity;
        CancelInvoke(); Invoke(nameof(Despawn), lifetime);
    }

    void OnCollisionEnter(Collision c)
    {
        // Ignore hits not in mask
        if (((1 << c.collider.gameObject.layer) & hitMask) == 0)
        { Despawn(); return; }

        // Check for Health component (player system)
        var hp = c.collider.GetComponent<Health>();
        if (hp) hp.ApplyDamage(damage);

        // Check for EnemyController component (enemy system)
        var enemyController = c.collider.GetComponent<EnemyController>();
        if (enemyController) enemyController.takeDamage(damage);

        Despawn();
    }

    void Despawn()
    {
        // For pooling later, replace with SetActive(false)
        Destroy(gameObject);
    }
}