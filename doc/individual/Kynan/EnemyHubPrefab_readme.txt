Prefab: Enemy Hub

A hub for all enemies to communicate to.
An instance of this prefab should be in any scene with enemies.

Runs centralized tasks which don't need to be done per enemy, such as:
  Pathfinding
  Terrain detection
  Tracking player actions
Also runs enemy tasks which need to outlive the enemy, such as:
  shooting bullets
And tasks which are easier done centrally, such as:
  Relaying hive messages
  Finding the nearest enemy to the player

The EnemyHub can be made by dragging the prefab into the scene.

The EnemyHub prefab comes attatched with a single script, EnemyHub.cs.
  The values which can be set in the inspector are:
    enemyTemplate (GameObject): the prefab of the enemy to spawn
    enemyBulletTemplate (GameObject): the prefab of the bullet to spawn
      These 2 values should not be changed
    actualMinX (float): the minimum x position of the terrain
    actualMaxX (float): the maximum x position of the terrain
    actualMinZ (float): the minimum z position of the terrain
    actualMaxZ (float): the maximum z position of the terrain
      These 4 values should not be smaller than the actual map size, or the pathfinding function may throw an error, but can be larger

  The public methods are:
    void runOnceMapLoads()
      Scans the map for pathfinding once the map has loaded
    void spawnEnemy(Vector3 position, string type, float strengthScaling, int hiveMemberID)
      Spawns an enemy at the given position with the given parameters (see the readme for the enemy prefab)
    void spawnEnemyAtTerrainHeight(Vector2 position, string type, float strengthScaling, int hiveMemberID)
      Spawns an enemy at the given position with the given parameters (see the readme for the enemy prefab), but uses the terrain height at the given position
    float getHeight(Vector2 position)
      Returns the height of the terrain at the given position (x and z positions for Vector2)
    Vector3 getClosestEnemy(Vector3 position, int onlyFlying)
      Returns the position of the closest enemy to the given position
      onlyFlying: 0 (default) for all enemies, 1 for only flying enemies, -1 for only non-flying enemies
    GameObject getPlayer()
      Returns the player
    
  Additional public methods which should only be called by Enemy or EnemyHub:
    GameObject createEnemyGameObject()
      Instantiates a new enemy game object. (This game object isn't connected to the Enemy class, and will not work)
    void addEnemy(Enemy enemy)
      Adds an enemy to the list of enemies
    Vector3 EnemyPathToPlayer(Vector3 position)
      Returns the path from the given position to the player
      This path goes around obstacles
      position: the position to start the path from
    void shoot(Vector3 position, Vector3 direction, float damage)
      Shoots an enemy bullet
      position: the position to spawn the bullet from
      direction: the direction (and speed) to shoot the bullet in
      damage: the amount of damage to deal to the player
    void shootMissile(Vector3 position, Vector3 direction, float damage, int homingStartFrame, int maxHomingFrames, float homingStrength)
      Shoots an enemy missile which homes in on the player
      position: the position to spawn the missile from
      direction: the direction (and speed) to shoot the missile in
      damage: the amount of damage to deal to the player
      homingStartFrame: the frame at which to start homing after spawning the missile
      maxHomingFrames: the frame at which to stop homing after spawning the missile
      homingStrength: the strength of the homing (how aggressively it turns)
    Sword sword(transform parentTransform, float range, float damage)
      Returns a new Sword object for a melee enemy to maintain
      parentTransform: the transform of the enemy's hand holding the sword
      range: the reach of the sword
      damage: the damage of the sword
    bool isPlayerMakingNoise()
      Returns true if the player shot at any enemy recently
    void relayHiveMessage(int hiveMemberID, string message)
      Relays a message within a hive
      hiveMemberID: the ID of the hive member to relay the message to
      message: the message to relay
        "shot-at": the player shot at an enemy, and it wants to warn the other enemies
        "spotted": an enemy has spotted the player
    void enemyDied(Enemy enemy, GameObject enemyInstance)
      Cleans up an enemy and removes its various parts

