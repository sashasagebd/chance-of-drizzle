using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyHub : MonoBehaviour{
  // Prefabs (set through editor)
  [SerializeField] private GameObject enemyTemplate;
  [SerializeField] private GameObject enemyBulletTemplate;

  [SerializeField] private float actualMinX = -47.0f;
  [SerializeField] private float actualMaxX =  47.0f;
  [SerializeField] private float actualMinZ = -47.0f;
  [SerializeField] private float actualMaxZ =  47.0f;

  // Terrain Analysis Variables
  private float maxTerrainHeight = 100.0f;
  private float minTerrainHeight = -1.0f;
  private float maxHeightDifference = 1.4f;
  private LayerMask rayMask;
  private float terrainMinX = -47.0f;
  private float terrainMaxX =  47.0f;
  private float terrainMinZ = -47.0f;
  private float terrainMaxZ =  47.0f;
  private float terrainPartitionStepSize = 1.5f;

  private int terrainWidth;
  private int terrainDepth;

  // Mathematical Constants (for hexagonal tiling)
  private const float SQRT3   = 1.7320508075688772f; // sqrt(3)
  private const float SQRT3_2 = 0.8660254037844386f; // sqrt(3) / 2
  private const float SQRT3_3 = 0.5773502691896258f; // sqrt(3) / 3
  private const float SQRT3_4 = 0.2886751345948129f; // sqrt(3) / 4

  // Variables for debugging / testing
  private bool debugVariablePrintHexagonalMap = false;
  private bool debugVariablePrintGroups = false;
  private bool debugVariablePrintDistanceToPlayer = false;

  // Variables for enemy spawning
  private List<Enemy> enemies = new List<Enemy>();

  // Variables for pathfinding / group generation
  private int maxGroupCount = 100;
  private float[,] terrainHeights;
  private int[,] terrainGroups;
  private int[,] distanceToPlayer;
  int[] playerHexagonalPosition;

  // lasers / missiles
  private List<Laser> lasers = new List<Laser>();
  private Color bulletColor = new Color(1.0f, 0.0f, 0.0f);

  // For spotting player
  private float enemyLastShotAtByPlayer = -1f;
  private float secondsToRememberLastShot = 2f;

  private int frameCount = 0;

  void Awake(){
    // Set up references to other scripts / GameObjects in this, Enemy, and Laser
    GameObject player = getPlayer();
    Laser.setStaticValues(player, this);
    Enemy.setStaticValues(player, this);

    // Set up raycast mask for terrain analysis
    rayMask = LayerMask.GetMask("Terrain");

    terrainWidth = (int)Mathf.Ceil((terrainMaxX - terrainMinX) / (terrainPartitionStepSize * SQRT3_2));
    terrainDepth = (int)Mathf.Ceil((terrainMaxZ - terrainMinZ) / (terrainPartitionStepSize));
  }
  void Start(){
    // Run terrain analysis in Start(), hopefully after terrain has been generated

    //debugVariablePrintHexagonalMap = true;
    //debugVariablePrintGroups = true;
    runOnceMapLoads();



    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "melee-figure-eight");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "melee-figure-eight-double");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "quad");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "melee");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "melee-egg-beater");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "homing-shot");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "basic");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "drizzle-of-doom");
    // 
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying-missile");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying-melee-quad");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying-melee");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying-double");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying-ufo");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying-sniper");
    // spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying-pyramid");

    for(int i = 0; i < 1; i++){
      Vector2 randomPosition = new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ));
      for(int j = 0; j < 3; j++){
        //Vector2 randomPosition2 = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        //spawnEnemyAtTerrainHeight(randomPosition + randomPosition2, "flying", 1f, i);
      }
    }
  }
  void Update(){
    frameCount++;
    enemyLastShotAtByPlayer -= Time.deltaTime;

    // Run pathfinding every 10 frames to minimize footprint
    if(frameCount % 10 == 0){
      if(frameCount % 600 == 0 && (
        terrainMinX != actualMinX ||
        terrainMaxX != actualMaxX ||
        terrainMinZ != actualMinZ ||
        terrainMaxZ != actualMaxZ
      )){
        runOnceMapLoads();
      }

      getPathToPlayer();
    }

    runLasers();
  }

  public void runOnceMapLoads(){
    GameObject player = getPlayer();

    terrainMinX = Mathf.Max(player.transform.position.x - 35f, actualMinX);
    terrainMaxX = Mathf.Min(player.transform.position.x + 35f, actualMaxX);
    terrainMinZ = Mathf.Max(player.transform.position.z - 35f, actualMinZ);
    terrainMaxZ = Mathf.Min(player.transform.position.z + 35f, actualMaxZ);

    terrainWidth = (int)Mathf.Ceil((terrainMaxX - terrainMinX) / (terrainPartitionStepSize * SQRT3_2));
    terrainDepth = (int)Mathf.Ceil((terrainMaxZ - terrainMinZ) / (terrainPartitionStepSize));
    
    getTerrain();
    getPathToPlayer();
  }
  public GameObject createEnemyGameObject(){
    // https://chamucode.com/unity-enemy-spawn/
    GameObject enemyInstance = Instantiate(enemyTemplate, Vector3.zero, Quaternion.identity);
    return enemyInstance;
  }
  public void addEnemy(Enemy enemy){
    enemies.Add(enemy);
  }
  public Enemy spawnEnemy(Vector3 position, string type = "melee", float strengthScaling = 1f, int hiveMemberID = -1){
    Enemy enemy = Enemy.createEnemy(position, type, strengthScaling, hiveMemberID);
    return enemy;
  }
  public Enemy spawnEnemyAtTerrainHeight(Vector2 position, string type = "melee", float strengthScaling = 1f, int hiveMemberID = -1){
    float height = getHeight(position);
    // Error if no terrain
    if(height > -99f){
      // Add to height for flying enemies
      if(Enemy.isFlying(type)){
        height += 3f;
      }
      return spawnEnemy(new Vector3(position.x, height + 1, position.y), type, strengthScaling, hiveMemberID);
    }
    print("Error: No terrain found at location. Is the map piece in layer terrain?");
    return null;
  }
  private void getTerrain(){
    // Split the map into hexagons, and get the heights of each hexagon
    terrainHeights = new float[terrainWidth, terrainDepth];
    for(int i = 0; i < terrainWidth; i++){
      for(int j = 0; j < terrainDepth; j++){
        float x = terrainMinX + i * terrainPartitionStepSize * SQRT3_2;
        float z = terrainMinZ + (j + (i % 2 > 0 ? 0.5f : 0f)) * terrainPartitionStepSize;
        terrainHeights[i, j] = -9e9f;
        // Keep only maximum height recorded out of 6 points
        for(float angle = 0f; angle < Mathf.PI * 2; angle += Mathf.PI / 3){
          terrainHeights[i, j] = Mathf.Max(
            getHeight(new Vector2(x + 0.2f * terrainPartitionStepSize * Mathf.Cos(angle), z + 0.2f * terrainPartitionStepSize * Mathf.Sin(angle))),
            terrainHeights[i, j]
          );
        }
        if(terrainHeights[i, j] < -99f){
          print("Error: The map parsing did not work (No ray intersection). Continuing as if unparsed section is wall (" + x + ", " + z + ")");
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
    
    // Populate terrainGroups with -1
    for(int i = 0; i < terrainWidth; i++){
      for(int j = 0; j < terrainDepth; j++){
        terrainGroups[i, j] = -1;
      }
    }

    // First row to check while grouping (older rows are ignored as they get grouped)
    int firstUnallocatedRow = 0;
    for(int currentGroupId = 0; currentGroupId < maxGroupCount; currentGroupId++){
      // Because C# doesn't have "break loopName;"
      bool continueLoop1 = false;
      bool breakLoop1 = false;

      // Creates new group instead of adding to existing group if false
      bool groupStarted = false;

      // Grouping variables for remembering selected region in last row and whether to increase the region
      bool groupGrowthLeft = true;
      bool groupGrowthRight = true;
      int longestUninterruptedSectionStart = 0;
      int longestUninterruptedSectionLength = 0;

      // Actual Algorithm for grouping
      for(int j = firstUnallocatedRow; j < terrainWidth; j++){
        if(!groupStarted){
          // Start a new group

          int currentUninterruptedSectionLength = 0;

          // Find longest uninterrupted section in current row
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
            // Make that region a new group
            // print(longestUninterruptedSectionStart + " " + longestUninterruptedSectionLength);
            for(int i = longestUninterruptedSectionStart; i < longestUninterruptedSectionStart + longestUninterruptedSectionLength; i++){
              terrainGroups[j, i] = currentGroupId;
            }
            groupStarted = true;
            continue;
          }else{
            // Continue to next row if no tiles were available
            firstUnallocatedRow = j + 1;
            
            // If there are no more rows to check, break
            if(firstUnallocatedRow == terrainDepth - 1){
              breakLoop1 = true;
              break;
            }
          }
        }else{
          // Offset for moving back and forth while traversing hexagons (why did I not use square tiles? AAAAH)
          int offset = 0;
          if(j % 2 == 0){
            longestUninterruptedSectionStart ++;
            offset = -1;
          }

          // Check if longest uninterrupted section is still uninterrupted (shaving off edges)
          longestUninterruptedSectionLength --;
          for(int i = longestUninterruptedSectionStart; i < longestUninterruptedSectionStart + longestUninterruptedSectionLength; i++){
            if(terrainGroups[j, i] != -1
            || Mathf.Abs(terrainHeights[j, i] - terrainHeights[j - 1, i + offset]) >= maxHeightDifference
            || Mathf.Abs(terrainHeights[j, i] - terrainHeights[j - 1, i + 1 + offset]) >= maxHeightDifference
            || (i > longestUninterruptedSectionStart && Mathf.Abs(terrainHeights[j, i] - terrainHeights[j, i - 1]) >= maxHeightDifference)
            ){
              // If not uninterrupted, continue to next group (and restart loop)
              continueLoop1 = true;
              break;
            }
          }
          if(!continueLoop1){
            // If uninterrupted, add to group
            for(int i = longestUninterruptedSectionStart; i < longestUninterruptedSectionStart + longestUninterruptedSectionLength; i++){
              terrainGroups[j, i] = currentGroupId;
            }

            // Grow group to the left if next left hexagon is available and of similar height
            // Can't grow if group already shrunk in previous row (to keep convexness)
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
              // Prevent future left growth if can't add and not at left edge (aka shrunk this row)
              groupGrowthLeft = false;
            }

            // Same for right
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

            // Special case for leftward growth if the group size was 0 in previous row
            // rightward version is handled above (with regular rightward growth)
            // Must be ran after all regular growth to prevent special growth occurring with regular growth (creates concave polygons)
            // Doesn't run if group grew right this row, because longestUninterruptedSectionStart wouldn't be 0
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

            // Start new group if group didn't grow this row
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
    // Converts real world position to hexagonal position
    // Does it the terrible way where it manually checks the distance to the 4 nearest hexagons
    // Definitely a better way to do it somewhere (TODO?)
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
    // Converts hexagonal position to real world position
    int i = hexagonalPosition[0];
    int j = hexagonalPosition[1];
    float x = terrainMinX + i * terrainPartitionStepSize * SQRT3_2;
    float z = terrainMinZ + (j + (i % 2 > 0 ? 0.5f : 0f)) * terrainPartitionStepSize;
    return new Vector2(x, z);
  }
  private int[,] reverseSearch(int[] hexagonalPosition){
    // Runs reverse search from player, so enemies can look at this map when pathfinding

    // Uses a garbage value if player is out of bounds (prevent the map from getting filled with error causing values)
    if(hexagonalPosition[0] < 0 || hexagonalPosition[1] < 0 || hexagonalPosition[0] >= terrainWidth || hexagonalPosition[1] >= terrainDepth){
      hexagonalPosition = new int[]{0, 0};
    }

    // Distance map (from player to each hexagon)
    int[,] distances = new int[terrainWidth, terrainDepth];

    // Populate distance map with -1
    for(int i = 0; i < terrainWidth; i++){
      for(int j = 0; j < terrainDepth; j++){
        distances[i, j] = -1;
      }
    }

    // Queue for current search nodes
    Queue<int[]> queue = new Queue<int[]>();
    
    // Start search from player
    queue.Enqueue(new int[]{hexagonalPosition[0], hexagonalPosition[1], 1});
    distances[hexagonalPosition[0], hexagonalPosition[1]] = 0;
    
    while(queue.Count > 0){
      // Get current node
      int[] node = queue.Dequeue();
      int i = node[0];
      int j = node[1];
      int offset = (i % 2 == 0 ? -1 : 0);

      // If not on top edge, add 2 upper hexagons to queue and give them values
      if(i > 0){
        // upper left exists, then add
        if(j + offset >= 0
        && distances[i - 1, j + offset] == -1
        && terrainHeights[i, j] > terrainHeights[i - 1, j + offset] - maxHeightDifference
        ){
          queue.Enqueue(new int[]{i - 1, j + offset, node[2] + 1});
          distances[i - 1, j + offset] = node[2];
        }
        // upper right exists, then add
        if(j + 1 + offset < terrainDepth
        && distances[i - 1, j + 1 + offset] == -1
        && terrainHeights[i, j] > terrainHeights[i - 1, j + 1 + offset] - maxHeightDifference
        ){
          queue.Enqueue(new int[]{i - 1, j + 1 + offset, node[2] + 1});
          distances[i - 1, j + 1 + offset] = node[2];
        }
      }
      // Do the same for lower hexagons
      if(i < terrainWidth - 1){
        // lower left
        if(j + offset >= 0
        && distances[i + 1, j + offset] == -1
        && terrainHeights[i, j] > terrainHeights[i + 1, j + offset] - maxHeightDifference
        ){
          queue.Enqueue(new int[]{i + 1, j + offset, node[2] + 1});
          distances[i + 1, j + offset] = node[2];
        }
        // lower right
        if(j + 1 + offset < terrainDepth
        && distances[i + 1, j + 1 + offset] == -1
        && terrainHeights[i, j] > terrainHeights[i + 1, j + 1 + offset] - maxHeightDifference
        ){
          queue.Enqueue(new int[]{i + 1, j + 1 + offset, node[2] + 1});
          distances[i + 1, j + 1 + offset] = node[2];
        }
      }
      // Add hexagon to left
      if(j > 0
      && distances[i, j - 1] == -1
      && terrainHeights[i, j] > terrainHeights[i, j - 1] - maxHeightDifference
      ){
        queue.Enqueue(new int[]{i, j - 1, node[2] + 1});
        distances[i, j - 1] = node[2];
      }
      // Add hexagon to right
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
    // Set up for reverse search to generate distance map
    GameObject player = getPlayer();
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
    // Uses the generated distance map to find the best path

    int i = hexagonalPosition[0];
    int j = hexagonalPosition[1];
    int offset = (i % 2 == 0 ? -1 : 0);

    int bestDistance = 99999;
    int[] bestNode = new int[2];

    // Check all adjacent hexagons to find which has lowest distance value (can't be unset, aka -1)
    // upper hexagons
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
    // lower hexagons
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
    // left hexagon
    if(j > 0
    && distanceToPlayer[i, j - 1] != -1
    && distanceToPlayer[i, j - 1] < bestDistance
    ){
      bestDistance = distanceToPlayer[i, j - 1];
      bestNode = new int[]{i, j - 1};
    }
    // right hexagon
    if(j < terrainDepth - 1
    && distanceToPlayer[i, j + 1] != -1
    && distanceToPlayer[i, j + 1] < bestDistance
    ){
      bestDistance = distanceToPlayer[i, j + 1];
      bestNode = new int[]{i, j + 1};
    }

    // Return nonvalue if no path is found
    if(bestDistance == 99999){
      return new Vector3(0, 0, 0);
    }

    // Use terrainGroups array to check if the enemy can move in a straight line in a convex shape
    // instead of zigzagging through every hexagon in the path
    // Makes the path look more natural and more efficient

    // If the next hexagon is not in the same group as the current one
    // then return the average position between the two (or the next node if enemy is already on this tile)
    // Stop checking for the rest of the path (short term goal already known, rest currently unnecessary)
    if(terrainGroups[i, j] != terrainGroups[bestNode[0], bestNode[1]]){
      Vector2 positionBest = getPositionFromHexagonalPosition(bestNode);
      if(iteration == 0){
        return new Vector3(positionBest.x, 0, positionBest.y);
      }
      Vector2 position = getPositionFromHexagonalPosition(hexagonalPosition);
      return new Vector3((position.x * 3 + positionBest.x) / 4, 0, (position.y * 3 + positionBest.y) / 4);
    }

    // If enemy is already next to player, return that tile
    // Also return the tile if max iteration count is reached (prevents stack overflow)
    if(bestDistance == 0 || iteration > 50){
      Vector2 positionBest = getPositionFromHexagonalPosition(bestNode);
      return new Vector3(positionBest.x, 0, positionBest.y);
    }

    // Otherwise, recursively investigate the best hexagon
    return getRecursivePath(bestNode, iteration + 1);
  }
  public Vector3 EnemyPathToPlayer(Vector3 position){
    // Get vector enemy should move to to reach player (pathfinding)
    // enemy's position in hexagonal tilespace
    int [] hexagonalPosition = getHexagonPosition(new Vector2(position.x, position.z));

    // If either enemy or player is out of bounds, return player position
    if(hexagonalPosition[0] < 0 || hexagonalPosition[0] >= terrainWidth || hexagonalPosition[1] < 0 || hexagonalPosition[1] >= terrainDepth
    || playerHexagonalPosition[0] < 0 || playerHexagonalPosition[0] >= terrainWidth || playerHexagonalPosition[1] < 0 || playerHexagonalPosition[1] >= terrainDepth){
      GameObject player = getPlayer();
      Vector3 playerPosition = new Vector3(player.transform.position.x, 0, player.transform.position.z);
      return playerPosition;
    }

    // If the two hexagons are in the same group, return player position (the group is convex, so enemy can move in a straight line without going out of group)
    if(terrainGroups[playerHexagonalPosition[0], playerHexagonalPosition[1]] == terrainGroups[hexagonalPosition[0], hexagonalPosition[1]]){
      GameObject player = getPlayer();
      Vector3 playerPosition = new Vector3(player.transform.position.x, 0, player.transform.position.z);
      return playerPosition;
    }

    // Otherwise, recursively investigate the best hexagon and return that
    return getRecursivePath(hexagonalPosition, 0);
  }
  public Vector3 getClosestEnemy(Vector3 position, int onlyFlying){
    // Get closest enemy position (used for demo-mode AI player)

    float closestDistance = 1e38f;
    Vector3 closestEnemy = new Vector3(0, 0, 0);
    // Check all enemies
    for(int i = 0; i < enemies.Count; i++){
      Enemy enemy = enemies[i];
      // If onlyFlying is 0, check all enemies
      // If onlyFlying is positive, check only flying enemies
      // If onlyFlying is negative, check only non-flying enemies
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
    // Add line renderer and GameObject to go with it (used for creating lasers and subclasses)
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
  protected bool runLaser(Laser laser, int i){
    // call laser's run function, and destroy it and its GameObject if it returns true
    if(laser.run()){
      Destroy(laser.getGameObject());
      lasers.RemoveAt(i);
      return true;
    }
    return false;
  }
  protected void runLasers(){
    // Run all lasers
    for(int i = 0; i < lasers.Count; i++){
      if(runLaser(lasers[i], i)){
        i--;
      }
    }
  }
  public void shoot(Vector3 position, Vector3 direction, float damage = 1f){
    // Fire laser
    lasers.Add(new Laser(addLineRenderer(), position, direction, damage));
  }
  public void shootMissile(Vector3 position, Vector3 direction, float damage = 1f, int homingStartFrame = 6, int maxHomingFrames = 60, float homingStrength = 0.14f){
    // Fire missile
    lasers.Add(new Missile(addLineRenderer(), position, direction, damage, homingStartFrame, maxHomingFrames, homingStrength));
  }
  public Sword sword(Transform parentTransform, float range, float damage = 1f){
    // Create sword
    return new Sword(addLineRenderer(), parentTransform, range, damage);
  }
  public bool isPlayerMakingNoise(){
    return enemyLastShotAtByPlayer > 0;
  }
  public void relayHiveMessage(int hiveMemberID, string message){
    if(message == "shot-at"){
      enemyLastShotAtByPlayer = secondsToRememberLastShot;
      if(hiveMemberID >= 0){
        for(int i = 0; i < enemies.Count; i++){
          if(enemies[i].getHiveMemberID() == hiveMemberID){
            enemies[i].hiveMemberShotAt();
          }
        }
      }
    }else if(message == "spotted"){
      if(hiveMemberID >= 0){
        for(int i = 0; i < enemies.Count; i++){
          if(enemies[i].getHiveMemberID() == hiveMemberID){
            enemies[i].hiveMemberSpotted();
          }
        }
      }
    }
  }


  public void enemyDied(Enemy enemy, GameObject enemyInstance){
    // Destroy enemy and remove it from the list
    Destroy(enemyInstance);
    enemies.Remove(enemy);
    //print("ENEMY DIED");
  }

  public GameObject getPlayer(){
    GameObject player = GameObject.Find("Player");
    if(player == null){
      player = GameObject.Find("Player ");
    }
    return player;
  }



  public int runTests(string testName){
    // Run tests (communicates with testing scripts to allow for testing of private methods)
    switch(testName){
      case "SPAWN_ENEMIES":
        return spawnEnemy(new Vector3(Random.Range(terrainMinX, terrainMaxX), 5f, Random.Range(terrainMinZ, terrainMaxZ))) == null ? 1 : 0;
      case "SPAWN_FLYING_ENEMIES":
        return spawnEnemy(new Vector3(Random.Range(terrainMinX, terrainMaxX), 5f, Random.Range(terrainMinZ, terrainMaxZ)), "flying") == null ? 1 : 0;
      case "SPAWN_ENEMIES_AT_TERRAIN_HEIGHT":
        return spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ))) == null ? 1 : 0;
      case "SPAWN_FLYING_ENEMIES_ABOVE_TERRAIN_HEIGHT":
        return spawnEnemyAtTerrainHeight(new Vector2(Random.Range(terrainMinX, terrainMaxX), Random.Range(terrainMinZ, terrainMaxZ)), "flying") == null ? 1 : 0;
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
        GameObject player = getPlayer();
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
