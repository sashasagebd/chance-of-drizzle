using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyHub : MonoBehaviour{
  public GameObject enemy;

  private float maxTerrainHeight = 20.0f;
  private float minTerrainHeight = -1.0f;
  private LayerMask rayMask;
  private int terrainWidth = 3;
  private int terrainDepth = 3;

  void Awake(){
    rayMask = LayerMask.GetMask("Terrain");
    // spawnEnemy(new Vector3(0, 0, 0));
    getTerrain();

    spawnEnemyAtTerrainHeight(new Vector2(-16, 4));
  }
  void Update(){
  }

  private void spawnEnemy(Vector3 position){
    // https://chamucode.com/unity-enemy-spawn/
    Instantiate(enemy, position, Quaternion.identity);
  }
  public void spawnEnemyAtTerrainHeight(Vector2 position){
    float height = getHeight(position);
    if(height > -99f){
      spawnEnemy(new Vector3(position.x, height + 1, position.y));
    }else{
      print("Error: No terrain found at location. Is the map piece in layer terrain?");
    }
  }
  private void getTerrain(){
    float[,] terrainHeights = new float[terrainWidth, terrainDepth];
    int i = 0;
    int j = 0;
    float x = 0.0f;
    float z = 2.0f;
    terrainHeights[i, j] = getHeight(new Vector2(x, z));
    if(terrainHeights[i, j] > -99f){
      spawnEnemy(new Vector3(x, terrainHeights[i, j] + 1, z));
    }else{
      print("Error: The map parsing did not work (No ray intersection). Continuing as if unparsed section is wall");
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
}
