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


            if (spawnerScript != null)
            
                if (spawnerScript.SpawnerRandomize())
                    if (spawnerScript is EnemySpawner enemyScript) enemyScript.Initialize();
                    else spawnerScript.Initialize();
                else Destroy(spawns);
            else {
                Debug.Log(spawns.name + " Not valid");
            }
        }
    }
}
