Prefab: ObjectSpawner

A spawner which determines what to spawn upon Awake (deleting itself if it can't spawn anything) and when initialized spawns an object from a list of random objects and then delete itself. 
The Initialize call is ran by LevelManager upon loading the level.

Values adjustable in the editor:
	protected bool spawnerRandomChance: used to determine if the comparison in SpawnerChoice() should be made
	protected int spawnerPriorityMin: lower value of spawner random chance. If the random value is <= to this one, then the spawner will spawn
	protected int spawnerPriorityMax: higher value of spawner random chance. Max value that the random integer can take in SpawnerChoice()
	
	protected bool spawnAtTerrainHeight: used in Initialize() to see if it should raycast downwards from the spawner and use that point instead of the spawner's position
	protected Vector3 constDisplace: set x,y,z displacement added onto the spawned object's position
	protected List<GameObject> toSpawn: list of prefabs/objects which the spawner will pick one of to spawn

The Public methods are: (should only be called by LevelManager)
	public bool SpawnerRandomize(): chooses a random value based on spawnerPriorityMax and spawnerPriorityMin values if spawnerRandomChance is true
	public virtual bool SpecificTest(): returns true. Used in Initialize() to determine if an object within toSpawn is valid. Function is meant to be overriden by subclasses for specialized behavior
	public virtual void Initialize(): spawns the object spawnChoice (determined at Awake) at either:
		- its Vector3 position 
		- or the result of a raycast downwards to terrain
		displaced by the value of constDisplace, then destroys itself