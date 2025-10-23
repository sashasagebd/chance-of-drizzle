using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyHub : MonoBehaviour{
  public GameObject enemy;

  private float maxTerrainHeight = 20.0f;
  private float minTerrainHeight = -1.0f;
  private LayerMask rayMask;
  private float terrainMinX = -17.0f;
  private float terrainMaxX =  12.0f;
  private float terrainMinZ = -12.0f;
  private float terrainMaxZ =  12.0f;
  private float terrainPartitionStepSize = 3.0f;

  private const float SQRT3   = 1.7320508075688772f; // sqrt(3)
  private const float SQRT3_2 = 0.8660254037844386f; // sqrt(3) / 2

  // Variables for debugging / testing
  private bool debugVariablePrintHexagonalMap = false;

  private int enemyCount = 0;

  void Awake(){
    rayMask = LayerMask.GetMask("Terrain");
    // spawnEnemy(new Vector3(0, 0, 0));
    
    //getTerrain();

    // spawnEnemyAtTerrainHeight(new Vector2(-16, 4));

    runTests("PRINT_HEXAGONAL_MAP");
    runTests("SPAWN_ENEMIES_AT_TERRAIN_HEIGHT");
  }
  void Update(){
  }

  private GameObject spawnEnemy(Vector3 position){
    // https://chamucode.com/unity-enemy-spawn/
    return Instantiate(enemy, position, Quaternion.identity);
  }
  public int spawnEnemyAtTerrainHeight(Vector2 position){
    float height = getHeight(position);
    if(height > -99f){
      GameObject enemyInstance = spawnEnemy(new Vector3(position.x, height + 1, position.y));
      if(enemyInstance == null){
        return 1;
      }
    }else{
      print("Error: No terrain found at location. Is the map piece in layer terrain?");
      return 1;
    }
    return 0;
  }
  private void getTerrain(){
    int terrainWidth = (int)Mathf.Ceil((terrainMaxX - terrainMinX) / terrainPartitionStepSize);
    int terrainDepth = (int)Mathf.Ceil((terrainMaxZ - terrainMinZ) / (terrainPartitionStepSize * SQRT3_2));
    
    // Split the map into hexagons, and get the heights of each hexagon
    float[,] terrainHeights = new float[terrainWidth, terrainDepth];
    for(int i = 0; i < terrainWidth; i++){
      for(int j = 0; j < terrainDepth; j++){
        float x = terrainMinX + i * terrainPartitionStepSize * SQRT3_2;
        float z = terrainMinZ + (j + (i % 2 > 0 ? 0.5f : 0f)) * terrainPartitionStepSize;
        terrainHeights[i, j] = -9e9f;
        for(float angle = 0f; angle < Mathf.PI * 2; angle += Mathf.PI / 3){
          terrainHeights[i, j] = Mathf.Max(
            getHeight(new Vector2(x + 0.2f * terrainPartitionStepSize * Mathf.Cos(angle), z + 0.2f * terrainPartitionStepSize * Mathf.Sin(angle))),
            terrainHeights[i, j]
          );
        }
        if(terrainHeights[i, j] < -99f){
          print("Error: The map parsing did not work (No ray intersection). Continuing as if unparsed section is wall");
        }
      }
    }

    if(debugVariablePrintHexagonalMap){
      string str = "";
      for(int i = 0; i < terrainWidth; i++){
        str += (i % 2 > 0 ? "\n " : "\n");
        for(int j = 0; j < terrainDepth; j++){
          str += Mathf.Floor(2 * terrainHeights[i, j]).ToString();
        }
      }
      print(str);
    }

    // Organize hexagons into convex chunks
    int[,] terrainGroups = new int[terrainWidth, terrainDepth];
    for(int j = 0; j < terrainDepth; j++){
      for(int i = 0; i < terrainWidth; i++){
        
      }
    }
  }
  private float getHeight(Vector2 position){
    // https://www.karvan1230.com/entry/2022/02/08/200731
    RaycastHit hit;
    if(Physics.Linecast(new Vector3(position.x, maxTerrainHeight, position.y), new Vector3(position.x, minTerrainHeight, position.y), out hit, rayMask)){
      return maxTerrainHeight - hit.distance;
    }else{
      return -100f;
    }
  }



  public int runTests(string testName){
    switch(testName){
      case "SPAWN_ENEMIES_AT_TERRAIN_HEIGHT":
        return spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)));
      break;
      case "CHECK_TERRAIN_EXISTS":
        for(int i = 0; i < 100; i++){
          if(getHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ))) < -99f){
            return 1;
          }
        }
      break;
    }
    return 0;
  }
}
