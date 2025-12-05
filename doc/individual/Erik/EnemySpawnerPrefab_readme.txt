Prefab: EnemySpawner

A spawner which determines what enemy to spawn upon Awake (out of a list of strings which are fed as ids into the EnemyHub class.) and when initialized by the
LevelManager, spawns an enemy (or a random number of enemies) at a point / within a range.

Certain values of ObjectSpawner like toSpawn and constDisplace are unused in favor of more specialized values.

Values adjustable in the editor:
	protected bool multipleEnemies: if true, randomizes the number of enemy spawns
	protected bool enemyHive: if true and multipleEnemies, increment hive count ID of every enemy spawned sequentially from 1
	protected int enemyCountMin: minimum random # of enemies to spawn. Can be equal to enemyCountMax
	protected int enemyCountMax: maximum random # of enemies to spawn. Can be equal to enemyCountMin
	protected int enemyDistanceVariance: maximum amount of distance around the spawning point in x,z that enemies can be randomly displaced from
	protected List<string> enemyType: list of the name of different enemy types that can spawn fed into EnemyHub, one is chosen and fed into private variable typeChoice at Awake

The Public methods are: (should only be called by LevelManager)
	public static void changeStrengthScaling(float newScale): changes the strengthScale value fed into EnemyHub in all spawners when their Initialize() functions are run
	public static void changeEnemyHub(EnemyHub hubReference): changes the EnemyHub that the EnemySpawners use in Initialize() to the chosen one (typically the only one in the game scene)
	public virtual void Initialize(): 
		- first determines a random x,z displacement using enemyDistanceVariance and -enemyDistanceVariance as the max bounds
		- calls EnemyHub's createEnemy function, spawning an enemy at the spawner position with the selected type and strength scaling
			- if spawnAtTerrainHeight from ObjectSpawner is true, calls spawnEnemyAtTerrainHeight from EnemyHub instead w/ the random displacement
		- if multipleEnemies is enabled, loop this function for a random amount of times between enemyCountMin - enemyCountMax (both inclusive)
		- then destroy the spawner
	- everything inhereted from ObjectSpawner