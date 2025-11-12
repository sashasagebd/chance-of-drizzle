using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : ObjectSpawner
{

    [Header("Difficulty scaling Settings")]
    [SerializeField] protected float strengthScaling = 1.0f;
    [SerializeField] protected int hiveMemID = -1;
    [Header("Multi-spawn Settings")]
    [SerializeField] protected bool multipleEnemies = false;
    [SerializeField] protected int enemyCountMin = 1;
    [SerializeField] protected int enemyCountMax = 1;
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

/*
    public override bool SpecificTest(GameObject testObject) {

        Debug.Log(testObject.GetComponent<EnemyController>());
        if (testObject.GetComponent<EnemyController>() != null) {
           
            // Debug.Log("True!");
            return true;
        } else {
            // Debug.Log("False!");
            return false;
        }

    }
    */

    public override void Initialize() {
        if (beenInitialized && typeChoice != "") {
            Debug.Log("Spawning object!");
            if (!multipleEnemies) {
                Debug.Log("Hello!");
                Enemy.createEnemy(transform.position,typeChoice,strengthScaling,hiveMemID);
                // Instantiate(spawnedEnemy, transform.position, Quaternion.identity);
            } 
            else {

            }
            Destroy(gameObject);
        }
    }

    public void changeStrengthScaling(float newScale) {
        strengthScaling = newScale;
    }
}
