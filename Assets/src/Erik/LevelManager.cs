using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    [Header("Level Settings")]
    [SerializeField] private float strengthScaling = 0.1f;
    [SerializeField] protected List<string> exitDestinations = new List<string> {"WinRoom"};

    static private List<string> playedLevels = new List<string>();

    async void Start() {
        await Task.Delay(100);
        // Debug.Log("Loading in objects...");
        // Debug.Log(GameObject.Find("Enemy Hub").name);

        trackPlayedLevels();

        EnemySpawner.changeStrengthScaling(strengthScaling);
        EnemySpawner.changeEnemyHub(GameObject.Find("Enemy Hub").GetComponent<EnemyHub>());

        string finalDestination;
        if (exitDestinations.Count>0) finalDestination = exitDestinations[Random.Range(0,exitDestinations.Count)];
        else {
            Debug.LogWarning("No exit location set in LevelManager! Defaulted to WinGame");
            finalDestination="WinGame";
        }
        GoalPoint.setExitDestination(finalDestination);

        GameObject spawners = GameObject.Find("Spawners");

        foreach (Transform sp in spawners.transform) {
            // Debug.Log("Child: " + sp.name);
            GameObject spawns = sp.gameObject;
            ObjectSpawner spawnerScript = spawns.GetComponent<ObjectSpawner>();
            SpawnerChoice spawnerChoice = spawns.GetComponent<SpawnerChoice>();

            if (spawnerScript != null)
                if (spawnerScript.SpawnerRandomize()) {

                    if (spawnerScript is EnemySpawner enemyScript) enemyScript.Initialize();
                    else if (spawnerScript is ItemSpawner itemScript) itemScript.Initialize();
                    else spawnerScript.Initialize();

                } else Destroy(spawns);

            else if (spawnerChoice != null) {
                spawnerChoice.Initialize();
            } else {
                Debug.LogWarning(spawns.name + " is not a valid spawner object!");
                Destroy(spawns);
            }
        }
    }

    static void trackPlayedLevels() {
        string currentScene = SceneManager.GetActiveScene().name;
        if (!playedLevels.Contains(currentScene))
        playedLevels.Add(currentScene);
        Debug.Log(currentScene);
    }
}
