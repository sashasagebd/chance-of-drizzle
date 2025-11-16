using System.Collections.Generic;
using UnityEngine;

public class SpawnerChoice : MonoBehaviour
{

    private List<GameObject> eligibleSpawners = new List<GameObject>();

    void Awake() {
        // Takes each child 
        foreach (Transform child in transform)
        {
            if (child!=null && child.GetComponent<ObjectSpawner>()!=null) {
                // Debug.Log("Adding child "+child.name);
                eligibleSpawners.Add(child.gameObject);
            }
        }
        if (eligibleSpawners.Count<=0) {
            Debug.LogWarning("SpawnerChoice could not properly spawn any objects!");
            Destroy(gameObject);
        } 
    }

    public void Initialize() {
        
        // for (in)
        int randIndex = Random.Range(0, eligibleSpawners.Count);
        GameObject designatedSpawner = eligibleSpawners[randIndex];
        if (designatedSpawner.GetComponent<ObjectSpawner>()!=null) {

            ObjectSpawner spawnerScript = designatedSpawner.GetComponent<ObjectSpawner>();
            
            if (spawnerScript is EnemySpawner enemyScript) enemyScript.Initialize();
            else if (spawnerScript is ItemSpawner itemScript) itemScript.Initialize();
             else spawnerScript.Initialize();

            eligibleSpawners.Remove(designatedSpawner);
        }

        // Debug.Log("SpawnerChoice is ago");
        Destroy(gameObject);

    }
}
