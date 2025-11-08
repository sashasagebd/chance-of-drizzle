using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyHub : MonoBehaviour{
  // Prefabs (set through editor)
  public GameObject enemyTemplate;
  public GameObject enemyBulletTemplate;

  private float maxTerrainHeight = 20.0f;
  private float minTerrainHeight = -1.0f;
  private float maxHeightDifference = 1.4f;
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
  private bool debugVariablePrintDistanceToPlayer = false;

  private int enemyCount = 0;
  private int maxGroupCount = 100;

  private float[,] terrainHeights;
  private int[,] terrainGroups;
  private int[,] distanceToPlayer;
  int[] playerHexagonalPosition;

  private List<Enemy> enemies = new List<Enemy>();
  private List<Laser> lasers = new List<Laser>();

  private Color bulletColor = new Color(1.0f, 0.0f, 0.0f);

  private int frameCount = 0;

  private AIPlayer aiPlayer;

  void Awake(){
    rayMask = LayerMask.GetMask("Terrain");
    
    //debugVariablePrintHexagonalMap = true;
    //debugVariablePrintGroups = true;
    getTerrain();

    //spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)));
    //spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)));
    //spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying");
    spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying");
    spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying-double");

    getPathToPlayer();

    aiPlayer = new AIPlayer(GameObject.Find("Player"), this);
    aiPlayer = null;

    Laser.setStaticValues(GameObject.Find("Player"), this);
  }
  void Update(){
    frameCount++;
    if(frameCount % 10 == 0){
      getPathToPlayer();
    }

    if(aiPlayer != null){
      if (aiPlayer.run()){
        aiPlayer = null;
      }
    }

    runLasers();
  }

  public GameObject createEnemyGameObject(){
    // https://chamucode.com/unity-enemy-spawn/
    GameObject enemyInstance = Instantiate(enemyTemplate, Vector3.zero, Quaternion.identity);
    return enemyInstance;
  }
  public void addEnemy(Enemy enemy){
    enemies.Add(enemy);
  }
  private Enemy spawnEnemy(Vector3 position, string type = "basic", float strengthScaling = 1f, int hiveMemberID = -1){
    Enemy enemy = Enemy.createEnemy(this, position, type, strengthScaling, hiveMemberID);
    return enemy;
  }
  public Enemy spawnEnemyAtTerrainHeight(Vector2 position, string type = "basic", float strengthScaling = 1f, int hiveMemberID = -1){
    float height = getHeight(position);
    if(height > -99f){
      if(Enemy.isFlying(type)){
        height += 3f;
      }
      return spawnEnemy(new Vector3(position.x, height + 1, position.y), type, strengthScaling, hiveMemberID);
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
  public float getHeight(Vector2 position){
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
  private Vector2 getPositionFromHexagonalPosition(int[] hexagonalPosition){
    int i = hexagonalPosition[0];
    int j = hexagonalPosition[1];
    float x = terrainMinX + i * terrainPartitionStepSize * SQRT3_2;
    float z = terrainMinZ + (j + (i % 2 > 0 ? 0.5f : 0f)) * terrainPartitionStepSize;
    return new Vector2(x, z);
  }
  private int[,] reverseSearch(int[] hexagonalPosition){
    int terrainWidth = (int)Mathf.Ceil((terrainMaxX - terrainMinX) / terrainPartitionStepSize);
    int terrainDepth = (int)Mathf.Ceil((terrainMaxZ - terrainMinZ) / (terrainPartitionStepSize * SQRT3_2));

    if(hexagonalPosition[0] < 0 || hexagonalPosition[1] < 0 || hexagonalPosition[0] >= terrainWidth || hexagonalPosition[1] >= terrainDepth){
      hexagonalPosition = new int[]{0, 0};
    }

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
        && terrainHeights[i, j] > terrainHeights[i - 1, j + offset] - maxHeightDifference
        ){
          queue.Enqueue(new int[]{i - 1, j + offset, node[2] + 1});
          distances[i - 1, j + offset] = node[2];
        }
        if(j + 1 + offset < terrainDepth
        && distances[i - 1, j + 1 + offset] == -1
        && terrainHeights[i, j] > terrainHeights[i - 1, j + 1 + offset] - maxHeightDifference
        ){
          queue.Enqueue(new int[]{i - 1, j + 1 + offset, node[2] + 1});
          distances[i - 1, j + 1 + offset] = node[2];
        }
      }
      if(i < terrainWidth - 1){
        if(j + offset >= 0
        && distances[i + 1, j + offset] == -1
        && terrainHeights[i, j] > terrainHeights[i + 1, j + offset] - maxHeightDifference
        ){
          queue.Enqueue(new int[]{i + 1, j + offset, node[2] + 1});
          distances[i + 1, j + offset] = node[2];
        }
        if(j + 1 + offset < terrainDepth
        && distances[i + 1, j + 1 + offset] == -1
        && terrainHeights[i, j] > terrainHeights[i + 1, j + 1 + offset] - maxHeightDifference
        ){
          queue.Enqueue(new int[]{i + 1, j + 1 + offset, node[2] + 1});
          distances[i + 1, j + 1 + offset] = node[2];
        }
      }
      if(j > 0
      && distances[i, j - 1] == -1
      && terrainHeights[i, j] > terrainHeights[i, j - 1] - maxHeightDifference
      ){
        queue.Enqueue(new int[]{i, j - 1, node[2] + 1});
        distances[i, j - 1] = node[2];
      }
      if(j < terrainDepth - 1
      && distances[i, j + 1] == -1
      && terrainHeights[i, j] > terrainHeights[i, j + 1] - maxHeightDifference
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
    playerHexagonalPosition = getHexagonPosition(playerPosition);

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
  private Vector3 getRecursivePath(int[] hexagonalPosition, int iteration){
    int terrainWidth = (int)Mathf.Ceil((terrainMaxX - terrainMinX) / terrainPartitionStepSize);
    int terrainDepth = (int)Mathf.Ceil((terrainMaxZ - terrainMinZ) / (terrainPartitionStepSize * SQRT3_2));

    int i = hexagonalPosition[0];
    int j = hexagonalPosition[1];
    int offset = (i % 2 == 0 ? -1 : 0);

    int bestDistance = 99999;
    int[] bestNode = new int[2];

    if(i > 0){
      if(j + offset >= 0
      && distanceToPlayer[i - 1, j + offset] != -1
      && distanceToPlayer[i - 1, j + offset] < bestDistance
      ){
        bestDistance = distanceToPlayer[i - 1, j + offset];
        bestNode = new int[]{i - 1, j + offset};
      }
      if(j + 1 + offset < terrainDepth
      && distanceToPlayer[i - 1, j + 1 + offset] != -1
      && distanceToPlayer[i - 1, j + 1 + offset] < bestDistance
      ){
        bestDistance = distanceToPlayer[i - 1, j + 1 + offset];
        bestNode = new int[]{i - 1, j + 1 + offset};
      }
    }
    if(i < terrainWidth - 1){
      if(j + offset >= 0
      && distanceToPlayer[i + 1, j + offset] != -1
      && distanceToPlayer[i + 1, j + offset] < bestDistance
      ){
        bestDistance = distanceToPlayer[i + 1, j + offset];
        bestNode = new int[]{i + 1, j + offset};
      }
      if(j + 1 + offset < terrainDepth
      && distanceToPlayer[i + 1, j + 1 + offset] != -1
      && distanceToPlayer[i + 1, j + 1 + offset] < bestDistance
      ){
        bestDistance = distanceToPlayer[i + 1, j + 1 + offset];
        bestNode = new int[]{i + 1, j + 1 + offset};
      }
    }
    if(j > 0
    && distanceToPlayer[i, j - 1] != -1
    && distanceToPlayer[i, j - 1] < bestDistance
    ){
      bestDistance = distanceToPlayer[i, j - 1];
      bestNode = new int[]{i, j - 1};
    }
    if(j < terrainDepth - 1
    && distanceToPlayer[i, j + 1] != -1
    && distanceToPlayer[i, j + 1] < bestDistance
    ){
      bestDistance = distanceToPlayer[i, j + 1];
      bestNode = new int[]{i, j + 1};
    }

    if(bestDistance == 99999){
      return new Vector3(0, 0, 0);
    }

    if(terrainGroups[i, j] != terrainGroups[bestNode[0], bestNode[1]]){
      Vector2 positionBest = getPositionFromHexagonalPosition(bestNode);
      if(iteration == 0){
        return new Vector3(positionBest.x, 0, positionBest.y);
      }
      Vector2 position = getPositionFromHexagonalPosition(hexagonalPosition);
      return new Vector3((position.x * 3 + positionBest.x) / 4, 0, (position.y * 3 + positionBest.y) / 4);
    }

    if(bestDistance == 0 || iteration > 50){
      Vector2 positionBest = getPositionFromHexagonalPosition(bestNode);
      return new Vector3(positionBest.x, 0, positionBest.y);
    }

    return getRecursivePath(bestNode, iteration + 1);
  }
  public Vector3 getPlayerPosition(){
    GameObject player = GameObject.Find("Player");
    return player.transform.position;
  }
  public Vector3 EnemyPathToPlayer(Vector3 position){
    int terrainWidth = (int)Mathf.Ceil((terrainMaxX - terrainMinX) / terrainPartitionStepSize);
    int terrainDepth = (int)Mathf.Ceil((terrainMaxZ - terrainMinZ) / (terrainPartitionStepSize * SQRT3_2));
    int [] hexagonalPosition = getHexagonPosition(new Vector2(position.x, position.z));

    if(hexagonalPosition[0] < 0 || hexagonalPosition[0] >= terrainWidth || hexagonalPosition[1] < 0 || hexagonalPosition[1] >= terrainDepth
    || playerHexagonalPosition[0] < 0 || playerHexagonalPosition[0] >= terrainWidth || playerHexagonalPosition[1] < 0 || playerHexagonalPosition[1] >= terrainDepth){
      GameObject player = GameObject.Find("Player");
      Vector3 playerPosition = new Vector3(player.transform.position.x, 0, player.transform.position.z);
      return playerPosition;
    }

    if(terrainGroups[playerHexagonalPosition[0], playerHexagonalPosition[1]] == terrainGroups[hexagonalPosition[0], hexagonalPosition[1]]){
      GameObject player = GameObject.Find("Player");
      Vector3 playerPosition = new Vector3(player.transform.position.x, 0, player.transform.position.z);
      return playerPosition;
    }

    // print("hexagonalPosition: " + hexagonalPosition[0] + ", " + hexagonalPosition[1] + " playerHexagonalPosition: " + playerHexagonalPosition[0] + ", " + playerHexagonalPosition[1]);

    return getRecursivePath(hexagonalPosition, 0);
  }
  public Vector3 getClosestEnemy(Vector3 position, int onlyFlying){
    float closestDistance = 1e38f;
    Vector3 closestEnemy = new Vector3(0, 0, 0);
    for(int i = 0; i < enemies.Count; i++){
      Enemy enemy = enemies[i];
      if(onlyFlying != 0){
        if((onlyFlying > 0) ^ (enemy is FlyingEnemy)){
          continue;
        }
      }
      Vector3 enemyPosition = enemy.getPosition();
      float distance = Mathf.Pow(enemyPosition.x - position.x, 2) + Mathf.Pow(enemyPosition.z - position.z, 2);
      if(distance < closestDistance){
        closestDistance = distance;
        closestEnemy = enemyPosition;
      }
    }
    return closestEnemy;
  }
  protected GameObject addLineRenderer(){
    GameObject bulletInstance = Instantiate(enemyBulletTemplate, Vector3.zero, Quaternion.identity);
    LineRenderer lineRenderer = bulletInstance.GetComponent<LineRenderer>();
    lineRenderer.startWidth = 0.03f;
    lineRenderer.endWidth = 0.03f;
    lineRenderer.positionCount = 2;
    lineRenderer.useWorldSpace = true;
    //lineRenderer.startColor = bulletColor;
    //lineRenderer.endColor = bulletColor;
    lineRenderer.enabled = true;
    return bulletInstance;
  }
  protected bool runLaser(Laser laser){
    if(laser.run()){
      Destroy(laser.getGameObject());
      lasers.Remove(laser);
      return true;
    }
    return false;
  }
  protected void runLasers(){
    for(int i = 0; i < lasers.Count; i++){
      if(runLaser(lasers[i])){
        i--;
      }
    }
  }
  public void shoot(Vector3 position, Vector3 direction){
    lasers.Add(new Laser(addLineRenderer(), position, direction));
  }


  public void enemyDied(Enemy enemy, GameObject enemyInstance){
    Destroy(enemyInstance);
    enemies.Remove(enemy);
    print("ENEMY DIED");
  }



  public int runTests(string testName){
    int terrainWidth = (int)Mathf.Ceil((terrainMaxX - terrainMinX) / terrainPartitionStepSize);
    int terrainDepth = (int)Mathf.Ceil((terrainMaxZ - terrainMinZ) / (terrainPartitionStepSize * SQRT3_2));
    switch(testName){
      case "SPAWN_ENEMIES":
        return spawnEnemy(new Vector3(Random.Range(terrainMinX, terrainMaxX), 5f, Random.Range(terrainMinZ, terrainMaxZ))) == null ? 1 : 0;
      break;
      case "SPAWN_FLYING_ENEMIES":
        return spawnEnemy(new Vector3(Random.Range(terrainMinX, terrainMaxX), 5f, Random.Range(terrainMinZ, terrainMaxZ)), "flying") == null ? 1 : 0;
      break;
      case "SPAWN_ENEMIES_AT_TERRAIN_HEIGHT":
        return spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ))) == null ? 1 : 0;
      break;
      case "SPAWN_FLYING_ENEMIES_ABOVE_TERRAIN_HEIGHT":
        return spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying") == null ? 1 : 0;
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
        debugVariablePrintHexagonalMap = false;
      break;
      case "DISPLAY_GROUPS":
        debugVariablePrintGroups = true;
        getTerrain();
        debugVariablePrintGroups = false;
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
      case "DISPLAY_DISTANCE_TO_PLAYER":
        debugVariablePrintDistanceToPlayer = true;
        getPathToPlayer();
        debugVariablePrintDistanceToPlayer = false;
      break;
    }
    return 0;
  }
}
