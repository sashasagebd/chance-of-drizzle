using UnityEngine;

public class TerrainGenerationTemp : MonoBehaviour{
  private Terrain terrain;

  [SerializeField] private bool loadItems = false;
  [SerializeField] private bool loadEnemies = false;

  private int edgeWidth = 4;

  float noiseSeedX = 0f;
  float noiseSeedY = 0f;

  float mapMinX = -100f;
  float mapMinZ = -100f;
  float mapMaxX = 100f;
  float mapMaxZ = 100f;

  Vector2 goalPosition;

  GameObject goal;

  void Awake(){
    terrain = GetComponent<Terrain>();

    noiseSeedX = Random.Range(0f, 100f);
    noiseSeedY = Random.Range(0f, 100f);

    goalPosition = new Vector2(Random.Range(0f, 1f) < 0.5f ? (mapMinX + 20f) : (mapMaxX - 20f), Random.Range(0f, 1f) < 0.5f ? (mapMinZ + 20f) : (mapMaxZ - 20f));

    generateTerrain();
  }

  void Start(){
    EnemyHub hub = GameObject.Find("Enemy Hub").GetComponent<EnemyHub>();
    hub.runOnceMapLoads();

    if(loadItems){
      GameObject items = GameObject.Find("Items");
      foreach(Transform child in items.transform){
        float x = Random.Range(mapMinX, mapMaxX);
        float z = Random.Range(mapMinZ, mapMaxZ);

        child.position = new Vector3(x, hub.getHeight(new Vector2(x, z)) + 1f, z);
      }
    }

    GameObject goal = GameObject.Find("Goal");
    goal.transform.position = new Vector3(goalPosition.x, hub.getHeight(new Vector2(goalPosition.x, goalPosition.y)) + 2f, goalPosition.y);
    print(goalPosition);

    float px = -goalPosition.x;
    float pz = -goalPosition.y + 5f;

    GameObject player = GameObject.Find("Player ");
    player.transform.position = new Vector3(px, hub.getHeight(new Vector2(px, pz)) + 1, pz);

    if(loadEnemies){
      string[] enemyTypes = {
        "basic",
        "flying",
        "melee",
        "flying-pyramid",
        "melee-figure-eight",
        "flying-double",
        "quad",
        "flying-melee-quad",
        "melee-figure-eight-double",
        "flying-sniper",
        "homing-shot",
        "flying-melee",
        "melee-egg-beater",
        "flying-ufo",
        "drizzle-of-doom",
        "flying-missile"
      };
      for(int i = 0; i < 40; i++){
        int typeIndex = Mathf.FloorToInt(Mathf.Pow(Random.Range(0f, 1f), 2.0f) * enemyTypes.Length);
        hub.spawnEnemyAtTerrainHeight(new Vector2(Random.Range(mapMinX, mapMaxX), Random.Range(mapMinZ, mapMaxZ)), enemyTypes[typeIndex]);
      }
    }
  }

  private void generateTerrain(){
    int resolution = terrain.terrainData.heightmapResolution;
    int aResolution = terrain.terrainData.alphamapResolution;

    float[,] heights = terrain.terrainData.GetHeights(0, 0, resolution, resolution);
    float[,,] splatmap = new float[aResolution, aResolution, 2];
    
    float goalHeight = getGoalHeight();

    for(int i = 0; i < resolution; i++){
      for(int j = 0; j < resolution; j++){
        float goalDistance = Mathf.Min(1f, Mathf.Max(0f, 2f - 0.1f * Vector2.Distance(new Vector2(mapMinX + (mapMaxX - mapMinX) * i / resolution, mapMinZ + (mapMaxZ - mapMinZ) * j / resolution), goalPosition)));

        float slopeGenerator = Mathf.Min(1f, perlinNoise(5f + 12f * i / resolution, 5f + 12f * j / resolution) * 1.75f - 0.25f);
        float cliffGenerator = perlinNoise(4f * i / resolution, 4f * j / resolution) * 4f;

        float overallHeight = (slopeGenerator * cliffGenerator + (1f - slopeGenerator) * Mathf.Floor(cliffGenerator)) * 0.25f;
        
        float roughness = perlinNoise(15f * i / resolution, 15f * j / resolution);

        heights[i, j] = goalHeight * goalDistance + (1f - goalDistance) * (0.6f * overallHeight + 0.2f * roughness);

        float cliffness = Mathf.Pow((cliffGenerator % 1f - 0.5f) * 2f, 8f);

        if(i < edgeWidth || j < edgeWidth || i > resolution - edgeWidth || j > resolution - edgeWidth){
          if(i == 0 || j == 0 || i == resolution - 1 || j == resolution - 1){
            heights[i, j] = 0.15f + 0.6f * cliffGenerator * 0.25f + 0.2f * roughness;
          }
          cliffness = 1f;
        }

        if(i == resolution - 1 || j == resolution - 1) continue;

        float textureNoise = perlinNoise(25f * i / resolution, 25f * j / resolution);
        float texturePercent = Mathf.Min(1f, Mathf.Max(0f, textureNoise * 0.8f + 0.85f * cliffness));

        splatmap[i, j, 0] = 1f - texturePercent;
        splatmap[i, j, 1] = texturePercent;
      }
    }
    terrain.terrainData.SetHeights(0, 0, heights);

    print(terrain.terrainData.terrainLayers.Length);

    terrain.terrainData.SetAlphamaps(0, 0, splatmap);
  }
  private float getGoalHeight(){
    int resolution = terrain.terrainData.heightmapResolution;

    int i = (int)((goalPosition.x - mapMinX) / (mapMaxX - mapMinX) * terrain.terrainData.heightmapResolution);
    int j = (int)((goalPosition.y - mapMinZ) / (mapMaxZ - mapMinZ) * terrain.terrainData.heightmapResolution);

    float slopeGenerator = Mathf.Min(1f, perlinNoise(5f + 12f * i / resolution, 5f + 12f * j / resolution) * 1.75f - 0.25f);
    float cliffGenerator = perlinNoise(4f * i / resolution, 4f * j / resolution) * 4f;

    float overallHeight = (slopeGenerator * cliffGenerator + (1f - slopeGenerator) * Mathf.Floor(cliffGenerator)) * 0.25f;
    
    float roughness = perlinNoise(15f * i / resolution, 15f * j / resolution);

    return 0.6f * overallHeight + 0.2f * roughness;
  }
  private float perlinNoise(float x, float y){
    return Mathf.PerlinNoise(noiseSeedX + x, noiseSeedY + y);
  }
  /*
  void Update(){
    
  }
  */
}
