Prefab: EnemyBullet

A bullet fired by an enemy, or an enemy sword depending on how it is set up.

Dragging the prefab into the scene will not create a working instance
Instead, it will be spawned by the EnemyHub, with 1 of the following enemyHub instance methods:
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

This prefab will be run by a Laser class, or one of its subclasses
  That class has no public methods meant to be used outside of EnemyHub.
