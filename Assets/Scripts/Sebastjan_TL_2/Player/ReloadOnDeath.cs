using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Health))]
public class ReloadOnDeath : MonoBehaviour {
    private Health _hp;
    void OnEnable() {
        _hp = GetComponent<Health>();
        _hp.OnDied += Reload;
    }
    void OnDisable() {
        if (_hp) _hp.OnDied -= Reload;
    }
    void Reload() {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}