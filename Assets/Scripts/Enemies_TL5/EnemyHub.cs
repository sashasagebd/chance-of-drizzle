using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyHub : MonoBehaviour{
  public GameObject enemyTemplate;

  private float maxTerrainHeight = 20.0f;
  private float minTerrainHeight = -1.0f;
  private LayerMask rayMask;
  private float terrainMinX = -17.0f;
  private float terrainMaxX =  12.0f;
  private float terrainMinZ = -12.0f;
  private float terrainMaxZ =  12.0f;
  private float terrainPartitionStepSize = 1.5f;

  private const float SQRT3   = 1.7320508075688772f; // sqrt(3)
  private const float SQRT3_2 = 0.8660254037844386f; // sqrt(3) / 2
  private const float SQRT3_3 = 0.5773502691896258f; // sqrt(3) / 3
  private const float SQRT3_4 = 0.2886751345948129f; // sqrt(3) / 4

  // Variables for debugging / testing
  private bool debugVariablePrintHexagonalMap = false;
  private bool debugVariablePrintGroups = false;
  private bool debugVariablePrintDistanceToPlayer = !false;

  private int enemyCount = 0;
  private int maxGroupCount = 100;

  private float[,] terrainHeights;
  private int[,] terrainGroups;
  private int[,] distanceToPlayer;

  private List<Enemy> enemies = new List<Enemy>();

  void Awake(){
    rayMask = LayerMask.GetMask("Terrain");
    
    //debugVariablePrintHexagonalMap = true;
    //debugVariablePrintGroups = true;
    getTerrain();

    runTests("SPAWN_ENEMIES_AT_TERRAIN_HEIGHT");

    getPathToPlayer();
  }
  void Update(){
  }

  private Enemy spawnEnemy(Vector3 position){
    // https://chamucode.com/unity-enemy-spawn/
    GameObject enemyInstance = Instantiate(enemyTemplate, position, Quaternion.identity);
    Enemy enemy = new Enemy(enemyInstance, position);
    enemies.Add(enemy);
    return enemy;
  }
  public Enemy spawnEnemyAtTerrainHeight(Vector2 position){
    float height = getHeight(position);
    if(height > -99f){
      return spawnEnemy(new Vector3(position.x, height + 1, position.y));
    }
    print("Error: No terrain found at location. Is the map piece in layer terrain?");
    return null;
  }
  private void getTerrain(){
    int terrainWidth = (int)Mathf.Ceil((terrainMaxX - terrainMinX) / terrainPartitionStepSize);
    int terrainDepth = (int)Mathf.Ceil((terrainMaxZ - terrainMinZ) / (terrainPartitionStepSize * SQRT3_2));
    
    // Split the map into hexagons, and get the heights of each hexagon
    terrainHeights = new float[terrainWidth, terrainDepth];
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
    terrainGroups = new int[terrainWidth, terrainDepth];
    for(int i = 0; i < terrainWidth; i++){
      for(int j = 0; j < terrainDepth; j++){
        terrainGroups[i, j] = -1;
      }
    }
    int firstUnallocatedRow = 0;
    float maxHeightDifference = 1f;

    for(int currentGroupId = 0; currentGroupId < maxGroupCount; currentGroupId++){
      bool continueLoop1 = false;
      bool groupStarted = false;
      bool breakLoop1 = false;

      bool groupGrowthLeft = true;
      bool groupGrowthRight = true;
      int longestUninterruptedSectionStart = 0;
      int longestUninterruptedSectionLength = 0;

      for(int j = firstUnallocatedRow; j < terrainWidth; j++){
        if(!groupStarted){
          int currentUninterruptedSectionLength = 0;
          for(int i = 0; i <= terrainDepth; i++){
            if(i < terrainDepth && terrainGroups[j, i] == -1 && (currentUninterruptedSectionLength == 0 || Mathf.Abs(terrainHeights[j, i] - terrainHeights[j, i - 1]) < maxHeightDifference)){
              currentUninterruptedSectionLength++;
            }else{
              if(currentUninterruptedSectionLength > longestUninterruptedSectionLength){
                longestUninterruptedSectionLength = currentUninterruptedSectionLength;
                longestUninterruptedSectionStart = i - currentUninterruptedSectionLength;
              }
              currentUninterruptedSectionLength = 0;
            }
          }
          if(longestUninterruptedSectionLength > 0){
            // print(longestUninterruptedSectionStart + " " + longestUninterruptedSectionLength);
            for(int i = longestUninterruptedSectionStart; i < longestUninterruptedSectionStart + longestUninterruptedSectionLength; i++){
              terrainGroups[j, i] = currentGroupId;
            }
            groupStarted = true;
            continue;
          }else{
            firstUnallocatedRow = j + 1;
            if(firstUnallocatedRow == terrainDepth - 1){
              breakLoop1 = true;
              break;
            }
          }
        }else{
          int offset = 0;
          if(j % 2 == 0){
            longestUninterruptedSectionStart ++;
            offset = -1;
          }
          longestUninterruptedSectionLength --;
          for(int i = longestUninterruptedSectionStart; i < longestUninterruptedSectionStart + longestUninterruptedSectionLength; i++){
            if(terrainGroups[j, i] != -1
            || Mathf.Abs(terrainHeights[j, i] - terrainHeights[j - 1, i + offset]) >= maxHeightDifference
            || Mathf.Abs(terrainHeights[j, i] - terrainHeights[j - 1, i + 1 + offset]) >= maxHeightDifference
            || (i > longestUninterruptedSectionStart && Mathf.Abs(terrainHeights[j, i] - terrainHeights[j, i - 1]) >= maxHeightDifference)
            ){
              continueLoop1 = true;
              break;
            }
          }
          if(!continueLoop1){
            for(int i = longestUninterruptedSectionStart; i < longestUninterruptedSectionStart + longestUninterruptedSectionLength; i++){
              terrainGroups[j, i] = currentGroupId;
            }
            // print(longestUninterruptedSectionStart + " " + longestUninterruptedSectionLength + " " + offset);
            bool wasGroupGrowthLeft = groupGrowthLeft;
            if(groupGrowthLeft
            && longestUninterruptedSectionStart > 0
            && terrainGroups[j, longestUninterruptedSectionStart - 1] == -1
            && Mathf.Abs(terrainHeights[j, longestUninterruptedSectionStart - 1] - terrainHeights[j - 1, longestUninterruptedSectionStart + offset]) < maxHeightDifference 
            && (longestUninterruptedSectionStart >= terrainDepth - 1 || Mathf.Abs(terrainHeights[j, longestUninterruptedSectionStart - 1] - terrainHeights[j, longestUninterruptedSectionStart]) < maxHeightDifference)
            ){
              terrainGroups[j, longestUninterruptedSectionStart - 1] = currentGroupId;
              longestUninterruptedSectionStart --;
              longestUninterruptedSectionLength ++;
            }else if(longestUninterruptedSectionStart > 0){
              groupGrowthLeft = false;
            }
            if(groupGrowthRight
            && longestUninterruptedSectionStart + longestUninterruptedSectionLength < terrainDepth
            && terrainGroups[j, longestUninterruptedSectionStart + longestUninterruptedSectionLength] == -1
            && Mathf.Abs(terrainHeights[j, longestUninterruptedSectionStart + longestUninterruptedSectionLength] - terrainHeights[j - 1, longestUninterruptedSectionStart + longestUninterruptedSectionLength + offset]) < maxHeightDifference
            && (longestUninterruptedSectionStart + longestUninterruptedSectionLength <= 0 || longestUninterruptedSectionLength == 0 || Mathf.Abs(terrainHeights[j, longestUninterruptedSectionStart + longestUninterruptedSectionLength] - terrainHeights[j, longestUninterruptedSectionStart + longestUninterruptedSectionLength - 1]) < maxHeightDifference)
            ){
              terrainGroups[j, longestUninterruptedSectionStart + longestUninterruptedSectionLength] = currentGroupId;
              longestUninterruptedSectionLength ++;
            }else if(longestUninterruptedSectionStart + longestUninterruptedSectionLength < terrainDepth){
              groupGrowthRight = false;
            }

            if(wasGroupGrowthLeft && longestUninterruptedSectionLength == 0 && !groupGrowthLeft
            && longestUninterruptedSectionStart > 0
            && terrainGroups[j, longestUninterruptedSectionStart - 1] == -1
            && Mathf.Abs(terrainHeights[j, longestUninterruptedSectionStart - 1] - terrainHeights[j - 1, longestUninterruptedSectionStart + offset]) < maxHeightDifference 
            ){
              terrainGroups[j, longestUninterruptedSectionStart - 1] = currentGroupId;
              longestUninterruptedSectionStart --;
              longestUninterruptedSectionLength ++;
              groupGrowthLeft = true;
            }

            if(longestUninterruptedSectionLength == 0){
              continueLoop1 = true;
            }
          }
        }
        if(continueLoop1){
          break;
        }
      }

      if(breakLoop1){
        break;
      }
    }


    if(debugVariablePrintGroups){
      string str = "";
      for(int i = 0; i < terrainWidth; i++){
        str += (i % 2 > 0 ? "\n " : "\n");
        for(int j = 0; j < terrainDepth; j++){
          str += (char)(65 + terrainGroups[i, j]);
        }
      }
      print(str);
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
  private int[] getHexagonPosition(Vector2 position){
    int nearestI = 0;
    int nearestJ = 0;
    float nearestDistance = Mathf.Infinity;

    int approximateX = (int)Mathf.Floor((position.x - terrainMinX) / (terrainPartitionStepSize * SQRT3_2));
    for(int i = approximateX; i < approximateX + 2; i++){
      int approximateZ = (int)Mathf.Floor((position.y - terrainMinZ) / terrainPartitionStepSize - (i % 2 > 0 ? 0.5f : 0f));
      for(int j = approximateZ; j < approximateZ + 2; j++){
        float x = terrainMinX + i * terrainPartitionStepSize * SQRT3_2;
        float z = terrainMinZ + (j + (i % 2 > 0 ? 0.5f : 0f)) * terrainPartitionStepSize;
        float distance = Vector2.Distance(new Vector2(x, z), position);

        if(distance < nearestDistance){
          nearestDistance = distance;
          nearestI = i;
          nearestJ = j;
        }
      }
    }
    return new int[]{nearestI, nearestJ};
  }
  private int[,] reverseSearch(int[] hexagonalPosition){
    int terrainWidth = (int)Mathf.Ceil((terrainMaxX - terrainMinX) / terrainPartitionStepSize);
    int terrainDepth = (int)Mathf.Ceil((terrainMaxZ - terrainMinZ) / (terrainPartitionStepSize * SQRT3_2));

    int[,] distances = new int[terrainWidth, terrainDepth];

    for(int i = 0; i < terrainWidth; i++){
      for(int j = 0; j < terrainDepth; j++){
        distances[i, j] = -1;
      }
    }
    Queue<int[]> queue = new Queue<int[]>();
    
    queue.Enqueue(new int[]{hexagonalPosition[0], hexagonalPosition[1], 1});
    distances[hexagonalPosition[0], hexagonalPosition[1]] = 0;
    
    while(queue.Count > 0){
      int[] node = queue.Dequeue();
      int i = node[0];
      int j = node[1];
      int offset = (i % 2 == 0 ? -1 : 0);
      if(i > 0){
        if(j + offset >= 0
        && distances[i - 1, j + offset] == -1
        //&& Mathf.Abs(terrainHeights[i, j] - terrainHeights[i - 1, j + offset]) < maxHeightDifference
        ){
          queue.Enqueue(new int[]{i - 1, j + offset, node[2] + 1});
          distances[i - 1, j + offset] = node[2];
        }
        if(j + 1 + offset < terrainDepth
        && distances[i - 1, j + 1 + offset] == -1
        //&& Mathf.Abs(terrainHeights[i, j] - terrainHeights[i - 1, j + 1 + offset]) < maxHeightDifference
        ){
          queue.Enqueue(new int[]{i - 1, j + 1 + offset, node[2] + 1});
          distances[i - 1, j + 1 + offset] = node[2];
        }
      }
      if(i < terrainWidth - 1){
        if(j + offset >= 0
        && distances[i + 1, j + offset] == -1
        //&& Mathf.Abs(terrainHeights[i, j] - terrainHeights[i + 1, j + offset]) < maxHeightDifference
        ){
          queue.Enqueue(new int[]{i + 1, j + offset, node[2] + 1});
          distances[i + 1, j + offset] = node[2];
        }
        if(j + 1 + offset < terrainDepth
        && distances[i + 1, j + 1 + offset] == -1
        //&& Mathf.Abs(terrainHeights[i, j] - terrainHeights[i + 1, j + 1 + offset]) < maxHeightDifference
        ){
          queue.Enqueue(new int[]{i + 1, j + 1 + offset, node[2] + 1});
          distances[i + 1, j + 1 + offset] = node[2];
        }
      }
      if(j > 0
      && distances[i, j - 1] == -1
      //&& Mathf.Abs(terrainHeights[i, j] - terrainHeights[i, j - 1]) < maxHeightDifference
      ){
        queue.Enqueue(new int[]{i, j - 1, node[2] + 1});
        distances[i, j - 1] = node[2];
      }
      if(j < terrainDepth - 1
      && distances[i, j + 1] == -1
      //&& Mathf.Abs(terrainHeights[i, j] - terrainHeights[i, j + 1]) < maxHeightDifference
      ){
        queue.Enqueue(new int[]{i, j + 1, node[2] + 1});
        distances[i, j + 1] = node[2];
      }
    }

    return distances;
  }
  private void getPathToPlayer(){
    int terrainWidth = (int)Mathf.Ceil((terrainMaxX - terrainMinX) / terrainPartitionStepSize);
    int terrainDepth = (int)Mathf.Ceil((terrainMaxZ - terrainMinZ) / (terrainPartitionStepSize * SQRT3_2));

    GameObject player = GameObject.Find("Player");
    Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.z);
    int[] playerHexagonalPosition = getHexagonPosition(playerPosition);

    distanceToPlayer = reverseSearch(playerHexagonalPosition);
    if(debugVariablePrintDistanceToPlayer){
      string str = "";
      for(int i = 0; i < terrainWidth; i++){
        str += (i % 2 > 0 ? "\n " : "\n");
        for(int j = 0; j < terrainDepth; j++){
          str += (char)(distanceToPlayer[i, j] + (int)('A'));
        }
      }
      print(str);
    }
  }



  public int runTests(string testName){
    int terrainWidth = (int)Mathf.Ceil((terrainMaxX - terrainMinX) / terrainPartitionStepSize);
    int terrainDepth = (int)Mathf.Ceil((terrainMaxZ - terrainMinZ) / (terrainPartitionStepSize * SQRT3_2));
    switch(testName){
      case "SPAWN_ENEMIES":
        return spawnEnemy(new Vector3(Random.Range(terrainMinX, terrainMaxX), 5f, Random.Range(terrainMinZ, terrainMaxZ))) == null ? 1 : 0;
      break;
      case "SPAWN_ENEMIES_AT_TERRAIN_HEIGHT":
        return spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ))) == null ? 1 : 0;
      break;
      case "CHECK_TERRAIN_EXISTS":
        for(int i = 0; i < 100; i++){
          if(getHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ))) < -99f){
            return 1;
          }
        }
      break;
      case "DISPLAY_HEXAGONAL_MAP":
        debugVariablePrintHexagonalMap = true;
        getTerrain();
      break;
      case "DISPLAY_GROUPS":
        debugVariablePrintGroups = true;
        getTerrain();
      break;
      case "TEST_GET_HEXAGON_POSITION":
        for(int i = 0; i < terrainWidth; i++){
          for(int j = 0; j < terrainDepth; j++){
            float x = terrainMinX + i * terrainPartitionStepSize * SQRT3_2;
            float z = terrainMinZ + (j + (i % 2 > 0 ? 0.5f : 0f)) * terrainPartitionStepSize;
            float angle = Random.Range(0f, 2f * Mathf.PI);
            Vector2 pos = new Vector2(x + 0.4f * terrainPartitionStepSize * Mathf.Cos(angle), z + 0.4f * terrainPartitionStepSize * Mathf.Sin(angle));
            int[] hex = getHexagonPosition(pos);
            if(hex[0] != i || hex[1] != j){
              print("Error: getHexagonPosition failed. Expected: (" + i + ", " + j + ") Got: (" + hex[0] + ", " + hex[1] + ") " + pos);
              return 1;
            }
            print("getHexagonPosition passed. Got: (" + hex[0] + ", " + hex[1] + ") " + pos);
          }
        }
      break;
      case "PLAYER_HEXAGONAL_POSITION":
        GameObject player = GameObject.Find("Player");
        if(player == null){
          return 1;
        }
        Vector2 playerPosition = new Vector2(player.transform.position.x, player.transform.position.z);
        int[] playerHexagonalPosition = getHexagonPosition(playerPosition);
        if(playerHexagonalPosition[0] < 0 || playerHexagonalPosition[1] < 0 || playerHexagonalPosition[0] >= terrainWidth || playerHexagonalPosition[1] >= terrainDepth){
          return 1;
        }
      break;
    }
    return 0;
  }
}
