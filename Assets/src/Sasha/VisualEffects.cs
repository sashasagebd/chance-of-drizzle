using UnityEngine;
using System.Collections.Generic;

public class VisualEffects : MonoBehaviour
{
    public static VisualEffects Instance;
    private Dictionary<string, GameObject> vfxPrefabs;
    public List<GameObject> effectPrefabs;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        vfxPrefabs = new Dictionary<string, GameObject>();

        foreach (var prefab in effectPrefabs)
        {
            if (prefab != null)
            {
                vfxPrefabs[prefab.name] = prefab;
            }
        }
    }
    public void PlayVisual(string vfxId, Vector3 position)
    {
        if (vfxPrefabs.TryGetValue(vfxId, out var prefab) && prefab != null)
        {
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);

            ParticleSystem ps = instance.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(instance, ps.main.duration);
        }
        else
        {
            Debug.LogError($"Visual Effect ID '{vfxId}' not found in library.");
        }
    }
}