using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    public int damage = 10;
    public float lifetime = 3f;
    public LayerMask hitMask; // include Hittable, exclude Weapon & Projectile
    Rigidbody _rb;

    void Awake() => _rb = GetComponent<Rigidbody>();

    public void Fire(Vector3 velocity)
    {
        _rb.linearVelocity = velocity;
        CancelInvoke(); Invoke(nameof(Despawn), lifetime);
    }

    void OnCollisionEnter(Collision c)
    {
        // Ignore hits not in mask
        if (((1 << c.collider.gameObject.layer) & hitMask) == 0)
        { Despawn(); return; }

        var hp = c.collider.GetComponent<Health>();
        if (hp) hp.ApplyDamage(damage);

        Despawn();
    }

    void Despawn()
    {
        // For pooling later, replace with SetActive(false)
        Destroy(gameObject);
    }
}