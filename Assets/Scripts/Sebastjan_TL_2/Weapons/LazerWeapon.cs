using System.Collections;
using UnityEngine;

public class LazerWeapon : WeaponBase
{
    [Header("Lazer")]
    public float range = 50f;
    public LayerMask hitMask;       // set to Hittable in Inspector
    public LineRenderer line;       // optional laser line
    public float lineTime = 0.05f;
    private WeaponAudio weaponAudio;

    protected override bool DoFire(Vector3 origin, Vector3 direction)
    {
        if (weaponAudio == null)
            weaponAudio = GetComponent<WeaponAudio>();
        weaponAudio?.OnWeaponFire(transform.position);

        Vector3 end = origin + direction.normalized * range;

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, range, hitMask))
        {
            end = hit.point;

            // Check for Health component (player system)
            var hp = hit.collider.GetComponent<Health>();
            if (hp) hp.ApplyDamage(damage + PlayerController3D.damageBonus);

            // Check for EnemyController component (enemy system)
            var enemyController = hit.collider.GetComponent<EnemyController>();
            if (enemyController) enemyController.takeDamage(damage + PlayerController3D.damageBonus);
        }

        if (line) StartCoroutine(FlashLine(origin, end));
        return true;
    }

    IEnumerator FlashLine(Vector3 a, Vector3 b)
    {
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.SetPosition(0, a);
        line.SetPosition(1, b);
        line.enabled = true;
        yield return new WaitForSeconds(lineTime);
        line.enabled = false;
    }
}