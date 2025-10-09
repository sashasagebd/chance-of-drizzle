using System.Collections;
using UnityEngine;

public class LazerWeapon : WeaponBase
{
    [Header("Lazer")]
    public float range = 50f;
    public LayerMask hitMask;       // set to Hittable in Inspector
    public LineRenderer line;       // optional laser line
    public float lineTime = 0.05f;

   
    protected override bool DoFire(Vector3 origin, Vector3 direction)
    {
        Vector3 end = origin + direction.normalized * range;

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, range, hitMask))
        {
            end = hit.point;
            var hp = hit.collider.GetComponent<Health>();
            if (hp) hp.ApplyDamage(damage);
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