using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : ObjectSpawner
{

    private static float strengthScaling = 0.1f;
    private static EnemyHub enemyHubReference;

    // [Header("Difficulty scaling Settings")]
    // [SerializeField] protected float strengthScaling = 1.0f;
    // [SerializeField] protected int hiveMemID = -1;
    [Header("Multi-spawn Settings")]
    [SerializeField] protected bool multipleEnemies = false;
    [SerializeField] protected bool enemyHive = false;
    [SerializeField] protected int enemyCountMin = 1;
    [SerializeField] protected int enemyCountMax = 1;
    [SerializeField] protected int enemyDistanceVariance = 1;
    [Header("EnemySpawner Randomization Settings")]
    [SerializeField] protected List<string> enemyType = new List<string>();

    private string typeChoice = "";

    void Awake() { // have to override awake for this one

        Transform visual = transform.Find("Visual");
        Destroy(visual.gameObject);

        if (enemyType.Count > 0) {
            int randIndex = Random.Range(0, enemyType.Count);
            typeChoice = enemyType[randIndex];
                // Debug.Log("Chose choice "+typeChoice.name+" at "+name);
            beenInitialized = true;
        }
    }

    public override void Initialize() {
        if (beenInitialized && typeChoice != "") {
            // Debug.Log("Spawning object!");

            Vector2 randXrange = new Vector2(transform.position.x-enemyDistanceVariance,transform.position.x+enemyDistanceVariance);
            Vector2 randZrange = new Vector2(transform.position.z-enemyDistanceVariance,transform.position.z+enemyDistanceVariance);
            if (!multipleEnemies) {
                // Debug.Log("Hello!");
                if (spawnAtTerrainHeight)
                {
                    enemyHubReference.spawnEnemyAtTerrainHeight(new Vector2(Random.Range(randXrange.x,randXrange.y),Random.Range(randZrange.x,randZrange.y)),typeChoice,strengthScaling,-1);
                } else {
                    Enemy.createEnemy(transform.position,typeChoice,strengthScaling,-1);
                }
            }  else {

                int randCount;
                int hiveCount = -1;

                if (enemyCountMin<enemyCountMax) randCount = Random.Range(enemyCountMin,enemyCountMax);
                else if (enemyCountMin>enemyCountMax) randCount = Random.Range(enemyCountMax,enemyCountMin);
                else randCount = enemyCountMax;

                for (int i=0; i<randCount; i++) {

                    if (enemyHive) hiveCount = i;
                    else hiveCount = -1;

                    if (spawnAtTerrainHeight)
                    {
                        enemyHubReference.spawnEnemyAtTerrainHeight(new Vector2(Random.Range(randXrange.x,randXrange.y),Random.Range(randZrange.x,randZrange.y)),typeChoice,strengthScaling,hiveCount);
                    } else {
                        Enemy.createEnemy(transform.position,typeChoice,strengthScaling,-1);
                    }
                }
                

            }
            Destroy(gameObject);
        }
    }

    public static void changeStrengthScaling(float newScale) {
        strengthScaling = newScale;
        // Debug.Log("Strength Scaling changed to "+newScale);
    }

    public static void changeEnemyHub(EnemyHub hubReference) {
        enemyHubReference = hubReference;
    }
}
