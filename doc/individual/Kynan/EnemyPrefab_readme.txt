Prefab: Enemy

An enemy game object that moves and attacks the player.
Dragging the enemy prefab into the scene will not create a working instance
Instead, spawn it by calling one of the following:
  enemyHub.spawnEnemy(Vector3 position, string type, float strengthScaling, int hiveMemberID);
  enemyHub.spawnEnemyAtTerrainHeight(Vector2 position, string type, float strengthScaling, int hiveMemberID);
  Enemy.createEnemy(Vector3 position, string type, float strengthScaling, int hiveMemberID);
    where enemyHub is a reference to the EnemyHub object in the scene
    Using any of these methods requires EnemyHub to be in the scene, and loaded. (Thus, these methods should be called in Start(), not Awake())

    position: the position to spawn the enemy at (x and z positions for Vector2)
    type: the type of enemy to spawn (types are):
      melee-figure-eight
      melee-figure-eight-double
      quad
      melee
      melee-egg-beater
      homing-shot
      basic
      drizzle-of-doom
      flying
      flying-missile
      flying-melee-quad
      flying-melee
      flying-double
      flying-ufo
      flying-sniper
      flying-pyramid
    strengthScaling: the strength scaling of the enemy (default 1.0, must be strictly greater than 0.0)
    hiveMemberID: the hive member ID of the enemy (default -1, any value larger will put the enemy in a hive, where they share information)
  
The Enemy prefab comes attatched with a single script, EnemyController.cs.
  The public methods are:
    void takeDamage(float damage)
      Relay to the Enemy class instance that the enemy has taken damage
      damage: the amount of damage to take

In the background, a largely hidden Enemy class controls all of the enemy logic.
  All communications with the Enemy class instance should be done through the EnemyController.
