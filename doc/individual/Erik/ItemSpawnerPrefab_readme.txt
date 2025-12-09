Prefab: ItemSpawner

A subclass of ObjectSpawner which spawns ItemPickup objects and performs
The Initialize call is ran by LevelManager upon loading the level.

The Public methods are: (should only be called by LevelManager)
	public virtual bool SpecificTest(Gameobject testObject): if testObject has the ItemPickup component, returns true.
	public virtual void Initialize(): Instantiates the item at either the raycast or normal position displaced by constDisplace depending on spawnAtTerrainHeight
		- For item icons to work, sets the pickup's public hudManager value to the Hud/HudManager object within the game scene
		- then destroy the spawner
	- everything inhereted from ObjectSpawner

Values adjustable in the editor:
	- everything inhereted from ObjectSpawner