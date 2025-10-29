using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] int spawnerPriority = 1;
    [SerializeField] int spawnerPriorityMax = 1;

    [SerializeField] int spawnCount = 1;
    
    [SerializeField] private List<GameObject> toSpawn = new List<GameObject>();
    // [SerializeField] private GameObject visual;

    private List<GameObject> validSpawn = new List<GameObject>();
    private GameObject spawnChoice = null;

    void Awake() {
        // Determine what from the list can be spawned
        // visual.SetActive(false);

        Transform visual = transform.Find("Visual");
        Destroy(visual.gameObject);

        foreach (var toSpawnObject in toSpawn) {
            if (toSpawnObject != null && SpecificTest()) {
                validSpawn.Add(toSpawnObject);
            }
        }

        int randIndex = Random.Range(0, validSpawn.Count);
        spawnChoice = validSpawn[randIndex];
    }

    public virtual bool SpecificTest() {
        // for subclasses. checks if class is valid for what the object spawner is trying to spawn
        return true;
    }
    
    public virtual void Initialize() {
        // Spawns random object within list


        if (spawnChoice != null) {
            for (int i = 0; i < spawnCount; i++) {
                Vector3 randomDisplace = new Vector3(Random.Range(-5,5),0,Random.Range(-5,5));

                Instantiate(spawnChoice, transform.position + randomDisplace, Quaternion.identity);
            }
        }
    }
}
