using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] protected int spawnerPriority = 1;
    [SerializeField] protected int spawnerPriorityMax = 1;

    [SerializeField] protected int spawnCount = 1;
    
    [SerializeField] protected List<GameObject> toSpawn = new List<GameObject>();
    // [SerializeField] private GameObject visual;

    protected List<GameObject> validSpawn = new List<GameObject>();
    protected GameObject spawnChoice = null;
    protected bool beenInitialized = false; 

    void Awake() {
        // Determine what from the list can be spawned
        // visual.SetActive(false);

        Transform visual = transform.Find("Visual");
        Destroy(visual.gameObject);

        foreach (GameObject toSpawnObject in toSpawn) {
            if (toSpawnObject != null && SpecificTest(toSpawnObject)) {
                Debug.Log("Adding " + toSpawnObject.name + " to validSpawn list of "+name);
                validSpawn.Add(toSpawnObject);
            }
        }

        if (validSpawn.Count > 0) {
            int randIndex = Random.Range(0, validSpawn.Count);
            spawnChoice = validSpawn[randIndex];
            // Debug.Log("Chose choice "+spawnChoice.name+" at "+name);
            beenInitialized = true;
        } else {
            Debug.LogWarning("Spawner could not properly spawn any objects!");
            Destroy(gameObject);
        }
    }

    public virtual bool SpecificTest(GameObject testObject) {
        // for subclasses. checks if class is valid for what the object spawner is trying to spawn
        return true;
    }
    
    public virtual void Initialize() {
        // Spawns random object within list

        // Debug.Log(beenInitialized + " at " + name);

        if (beenInitialized && spawnChoice != null) {
            Debug.Log("Spawning objects!");
            for (int i = 0; i < spawnCount; i++) {
                Vector3 randomDisplace = new Vector3(Random.Range(-5,5),0,Random.Range(-5,5));

                Instantiate(spawnChoice, transform.position + randomDisplace, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}
