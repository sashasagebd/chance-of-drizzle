using UnityEngine;

using System.Threading.Tasks;

public class LevelManager : MonoBehaviour
{
    async void Start() {
        await Task.Delay(100);
        // Debug.Log("Loading in objects...");

        GameObject spawners = GameObject.Find("Spawners");

        foreach (Transform sp in spawners.transform) {
            // Debug.Log("Child: " + sp.name);
            GameObject spawns = sp.gameObject;
            ObjectSpawner spawnerScript = spawns.GetComponent<ObjectSpawner>();
            SpawnerChoice spawnerChoice = spawns.GetComponent<SpawnerChoice>();

            if (spawnerScript != null)
                if (spawnerScript.SpawnerRandomize())
                    if (spawnerScript is EnemySpawner enemyScript) enemyScript.Initialize();
                    else if (spawnerScript is ItemSpawner itemScript) itemScript.Initialize();
                    else spawnerScript.Initialize();
                else Destroy(spawns);
            else if (spawnerChoice != null) {
                spawnerChoice.Initialize();
            } else {
                Debug.LogWarning(spawns.name + " is not valid");
            }
        }
    }
}
