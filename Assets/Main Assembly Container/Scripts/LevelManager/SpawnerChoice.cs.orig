using System.Collections.Generic;
using UnityEngine;

public class SpawnerChoice : MonoBehaviour
{

    private List<GameObject> eligibleSpawners = new List<GameObject>();

    void Awake() {
        foreach (Transform child in transform)
        {
            if (child!=null && child.GetComponent<ObjectSpawner>()!=null) {
                Debug.Log("Adding child "+child.name);
                eligibleSpawners.Add(child.gameObject);
            }
        }
        if (eligibleSpawners.Count<=0) {
            Debug.LogWarning("SpawnerChoice could not properly spawn any objects!");
            Destroy(gameObject);
        } 
    }

    public void Initialize() {
        
        int randIndex = Random.Range(0, eligibleSpawners.Count);
        GameObject designatedSpawner = eligibleSpawners[randIndex];
        if (designatedSpawner.GetComponent<ObjectSpawner>()!=null)
            eligibleSpawners[randIndex].GetComponent<ObjectSpawner>().Initialize();

        Destroy(gameObject);

    }
}
