using UnityEngine;

public class Hazard : MonoBehaviour {
    [SerializeField] private int dps = 25;  // damage per second
    void OnTriggerStay(Collider other) {
        var hp = other.GetComponent<Health>();
        if (!hp) return;
        hp.ApplyDamage(Mathf.CeilToInt(dps * Time.deltaTime));
    }
}