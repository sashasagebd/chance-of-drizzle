Prefab: LevelManager

A singleton loaded into every level scene which handles randomizing spawns at initial runtime
Does not have any functions to call, as runs once upon opening the scene and does not do anything else.

in Start():
	Runs static command within EnemySpawner changeStrengthScaling() which changes the strength scaling value that all EnemySpawners use
	and changeEnemyHub() to locate the GameObject EnemyHub which EnemySpawner relies on to call.

	Runs static command within GoalPoint setExitDestination(string finalDestination) which sets the name of the scene that
	all goal posts will load upon contact.

	Performs randomization by looping through every grandchild within "Spawners", 
	first determining if each object has an ObjectSpawner or a SpawnerChoice component, then
	running Initialize() if it is a SpawnerChoice or if the spawner first returns a true value from a SpawnerRandomize() call.
	If the object has neither components, it is deleted and a warning is logged.

Values adjustable in the editor:
	float StrengthScaling (default 0.1)
	List<string> exitDestinations (default is just a list with "WinRoom", the winscreen)