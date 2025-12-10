using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    [Header("Level Settings")]
    // [SerializeField] private float strengthScaling = 0.1f;
    [SerializeField] protected List<string> exitDestinations = new List<string>();

    static private float strengthScaling = 0.2f;
    static private List<string> playedLevels = new List<string>();

    async void Start() {
        await Task.Delay(100);

        EnemySpawner.changeStrengthScaling(strengthScaling);
        EnemySpawner.changeEnemyHub(GameObject.Find("Enemy Hub").GetComponent<EnemyHub>());

        // iterate through levels which have been already played to remove from the exitDestinations list
        foreach (string playedDestination in playedLevels) {
            for (int i = exitDestinations.Count-1; i>=0; i--) {
                string potentialDestination = exitDestinations[i];
                if (potentialDestination == playedDestination) exitDestinations.Remove(potentialDestination);
            }
        }

        string finalDestination;
        if (playedLevels.Count >= 2) finalDestination = "WinGame";
        else if (exitDestinations.Count>0) finalDestination = exitDestinations[Random.Range(0,exitDestinations.Count)];
        else {
            Debug.LogWarning("No exit location set in LevelManager! Defaulted to WinGame");
            finalDestination="WinGame";
        }
        GoalPoint.setExitDestination(finalDestination);

        trackPlayedLevels();

        GameObject spawners = GameObject.Find("Spawners");

        // iterate through all objects in Spawners sub-object
        foreach (Transform sp in spawners.transform) {
            // Debug.Log("Child: " + sp.name);
            GameObject spawns = sp.gameObject;
            ObjectSpawner spawnerScript = spawns.GetComponent<ObjectSpawner>();
            SpawnerChoice spawnerChoice = spawns.GetComponent<SpawnerChoice>();

            // if the object is a spawner, then run its initialize command (based on its class)
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
        // upon entering level, checks current loaded scene (level) and adds it to static list
        // list is used for tracking what levels the player has played thus far

        string currentScene = SceneManager.GetActiveScene().name;

        if (!playedLevels.Contains(currentScene))
        playedLevels.Add(currentScene);

        strengthScaling += 0.2f; // Increment strength scaling so enemies deal more damage and get harder over time.

        Debug.Log("Strength Scaling: "+strengthScaling);
        foreach (string sceneId in playedLevels) {
            Debug.Log("Played: "+sceneId);
        }
    }
}
