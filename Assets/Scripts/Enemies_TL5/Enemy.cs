using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy{
  // References to other scripts / GameObjects
  protected static EnemyHub enemyHub;
  protected static GameObject player;

  // Offset of player head from player position
  protected static Vector3 playerHead = new Vector3(0, 0.8f, 0);

  // Game object / Unity integration
  protected GameObject enemy;
  protected EnemyController enemyController;
  protected Rigidbody rb;

  // Used for jumping / calculating if on ground
  protected float maxJumpHeight = 1.6f;
  protected float jumpStrength = 7f;
  protected int stayedStillCount = 0;
  protected int frameCount = 0;

  // Basic stats
  protected float health = 0f;
  protected float maxHealth = 0f;
  protected float damage = 0f;
  protected float reloadTime  = 1e3f;
  protected float movementSpeed = 0f;
  protected float wanderSpeed = 0f;

  // Weapon stats
  protected float reloadTimer = 0f;
  protected float range = 20f;
  protected bool alternateGuns = true;
  protected float accuracy = 3f;
  protected float firingFreedom = 5f;
  protected int lastFired = 0;
  protected float shotSpeed = 1.35f;
  protected bool overrideDirection = false;
  protected Quaternion firingDirectionOverride = Quaternion.identity;

  // Homing missile behaviour
  protected bool homing = false;
  protected int homingStartFrame = 6;
  protected int maxHomingFrames = 60;
  protected float homingStrength = 0.14f;

  // Enemy behaviour and whether to shoot
  protected bool stopWhenInRange = true;
  protected bool keepMoving = true;
  protected bool checkIfCanShoot = true;
  protected bool canShoot = false;
  protected bool playerInSight = false;
  protected const int checkInterval = 10;
  protected int checkOffset;
  protected Vector3 head = new Vector3(0, 0.7f, 0);
  protected bool checkAllGuns = false;
  protected float noiseSeed = 0f;

  // spotting player and what enemy knows
  protected bool alwaysAttack = false;
  protected bool knowsPlayerLocation = false;
  protected float timeToForget = 0f;
  protected float maxMemoryVision = 10f;
  protected float maxMemoryAttacked = 25f;
  protected float focusedFocalRange = 0f; // Keeps track of player if enemy already knows about player and player is in this range
  protected float focalRange = 0f;        // Finds player if within focal triangle and in this range (view can't be obstructed)
  protected float hearingRange = 0f;      // Finds player if within this range and player shoots at any enemy
  protected float absoluteRange = 0f;     // Finds player if within this range
  protected float focalAngle = 25f;       // Uses focal triangle (only xz values, ignoring y)
  protected Vector3 lastWanderCenter = new Vector2(0f, 0f);
  protected bool setNewWanderCenter = true;
  protected int hiveMemberID = -1;

  // Weapon movement (orbit around enemy)
  protected float weaponSpinSpeed = 7f;
  protected float gunPositionDistance;
  protected float gunWobbleDistance;
  protected float gunInPlaceRadius = 1f;

  // Weapon spin mode (bitwise flags)
  protected const int spinX = 1;
  protected const int spinY = 2;
  protected const int spinZ = 4;
  protected const int spinWobble = 8;
  protected const int spinAlternate = 16;
  protected const int spinInPlace = 32;
  protected int spinMode = 0;

  // Time delay randomness for all spinning
  protected float timeDelay = 0f;

  // List of where the weapons fire from
  protected List<Transform> gunPositions = new List<Transform>();

  //protected Vector3 velocity;
  public Enemy(Vector3 position, string type, float strengthScaling, int hiveMemberID){
    // Set up Unity integration
    this.enemy = Enemy.enemyHub.createEnemyGameObject();
    this.enemy.transform.position = position;
    this.enemyController = this.enemy.GetComponent<EnemyController>();
    this.enemyController.enemy = this;
    this.rb = this.enemy.GetComponent<Rigidbody>();

    // Find and record weapon positions
    Transform objT = this.enemy.transform.Find(type);
    GameObject obj = objT.gameObject;
    obj.SetActive(true);
    foreach (Transform gunPosition in objT) {
      this.gunPositions.Add(gunPosition);
    }
    this.weaponSpinSpeed = (Random.Range(0f, 1f) < 0.5f ? this.weaponSpinSpeed : -this.weaponSpinSpeed) * (Random.Range(0.7f, 1.2f));

    // Define offsets to prevent all enemies doing the same thing at the same time
    this.timeDelay = Random.Range(0f, 10f);
    this.checkOffset = Random.Range(0, Enemy.checkInterval);
    this.noiseSeed = Random.Range(0f, 1000f);

    // Set hive membership
    this.hiveMemberID = hiveMemberID;

    // Individual stats for various enemy types
    switch(type){
      case "quad":
        this.movementSpeed = 0.8f;
        this.damage = 2.3f;
        this.maxHealth = 100f;
        this.reloadTime = 1.5f;
        this.alternateGuns = false;
        this.accuracy = 4.5f;
      break;
      case "homing-shot":
        this.movementSpeed = 0.7f;
        this.damage = 5f;
        this.maxHealth = 80f;
        this.reloadTime = 2.2f;
        this.accuracy = 0f;
        this.homing = true;
        this.shotSpeed = 0.37f;
        this.range *= 1.3f;
        this.maxHomingFrames = 190;
        this.homingStrength = 0.05f;
      break;
      case "basic":
        this.movementSpeed = 1f;
        this.damage = 2.6f;
        this.maxHealth = 90f;
        this.reloadTime = 1f;
        this.range *= 0.8f;
      break;
      case "drizzle-of-doom":
        this.movementSpeed = 0.65f;
        this.damage = 1f;
        this.maxHealth = 70f;
        this.reloadTime = 0.12f;
        this.accuracy = 14f;
        this.homing = true;
        this.shotSpeed = 0.6f;
        this.range *= 0.9f;
        this.spinMode = Enemy.spinX | Enemy.spinY;
        this.overrideDirection = true;
        this.firingDirectionOverride = Quaternion.LookRotation(Vector3.up);
        this.checkIfCanShoot = false;
        this.homingStartFrame = 65;
        this.maxHomingFrames = 145;
        this.homingStrength = 0.12f;
      break;
    }

    // Run setup functions
    this.applyStrengthScaling(strengthScaling);
    this.setGunPositionDistance();
    this.setFindRanges();

    // Record self in hub
    Enemy.enemyHub.addEnemy(this);
  }
  protected void setGunPositionDistance(){
    // Find gun spin and wobble radii
    float x = this.gunPositions[0].localPosition.x * ((this.spinMode & Enemy.spinX) > 0 ? 1f : 0f);
    float y = this.gunPositions[0].localPosition.y * ((this.spinMode & Enemy.spinY) > 0 ? 1f : 0f);
    float z = this.gunPositions[0].localPosition.z * ((this.spinMode & Enemy.spinZ) > 0 ? 1f : 0f);
    this.gunPositionDistance = Mathf.Sqrt(x * x + y * y + z * z);
    this.gunWobbleDistance = (new Vector3(x, y, z) - this.gunPositions[0].localPosition).magnitude;
  }
  protected void applyStrengthScaling(float strengthScaling){
    this.damage *= strengthScaling;
    this.maxHealth *= (strengthScaling + 2f) / 3f;
    this.movementSpeed *= (Mathf.Log(strengthScaling) + 4f) / 4f;
    this.reloadTime /= (Mathf.Log(strengthScaling) + 6f) / 6f;

    this.health = this.maxHealth;
    this.wanderSpeed = this.movementSpeed * 0.4f;
  }
  protected void setFindRanges(){
    this.focusedFocalRange = this.range * 2.6f;
    this.focalRange        = this.range * 1.9f;
    this.hearingRange      = this.range * 1.3f;
    this.absoluteRange     = this.range * 0.7f;
  }
  public void updateStayedStillCount(){
    // Increase count when minimal y velocity (reads as on ground if still for long enough)
    if(Mathf.Abs(this.rb.linearVelocity.y) < 0.05f){
      this.stayedStillCount++;
    }else{
      this.stayedStillCount = 0;
    }
  }
  protected float getTerrainHeight(){
    return Enemy.enemyHub.getHeight(new Vector2(this.enemy.transform.position.x, this.enemy.transform.position.z));
  }
  protected bool isOnGround(){
    float terrainHeight = this.getTerrainHeight();
    if(Mathf.Abs(terrainHeight - (this.enemy.transform.position.y - 1f)) < 0.05f || stayedStillCount > 7){
      return true;
    }
    return false;
  }
  protected float getHeightInFront(Vector3 moveDir){
    Vector3 forward = new Vector3(moveDir.x, 0f, moveDir.z).normalized * 0.55f + this.enemy.transform.position;
    return Enemy.enemyHub.getHeight(new Vector2(forward.x, forward.z));
  }
  protected float obstacleInFront(Vector3 moveDir){//, float strictness = 0.2f){
    float terrainHeight = this.getHeightInFront(moveDir);
    return terrainHeight - (this.enemy.transform.position.y - 1f);
    /*if(terrainHeight - (this.enemy.transform.position.y - 1f) > strictness){
    //&& terrainHeight - (this.enemy.transform.position.y - 1f) < this.maxJumpHeight){
      return true;
    }
    return false;*/
  }
  protected float sigmoid(float x){
    return 1f / (1f + Mathf.Exp(-x));
  }
  protected virtual void move(){
    Vector3 toPlayerPosition = Enemy.enemyHub.EnemyPathToPlayer(this.enemy.transform.position);
    Vector3 acceleration = toPlayerPosition - this.enemy.transform.position;

    Vector3 toPlayer = Enemy.player.transform.position - this.enemy.transform.position;

    if(this.stopWhenInRange && toPlayer.magnitude < this.range && !this.keepMoving && this.canShoot){
      // Used for facing towards player when in range (doesn't actually move)
      acceleration = new Vector3(toPlayer.x, 0f, toPlayer.z);
    }else if(this.stopWhenInRange && toPlayer.magnitude < this.range * 0.7f && this.canShoot){
      // Only stop moving when player very close (then don't start again until entirely out of range)
      this.keepMoving = false;
    }else{
      this.keepMoving = true;

      // Only move on xz plane
      acceleration = acceleration.normalized * this.movementSpeed;
      acceleration = new Vector3(acceleration.x, 0f, acceleration.z);
      
      bool grounded = this.isOnGround();
      float obstacleHeight = this.obstacleInFront(acceleration);

      // Regular movement
      if(grounded || obstacleHeight <= 0f){
        this.rb.linearVelocity = new Vector3(0.75f * this.rb.linearVelocity.x, this.rb.linearVelocity.y, 0.75f * this.rb.linearVelocity.z) + acceleration;
      }

      // Jump
      if(grounded && obstacleHeight > 0.2f){
        this.rb.linearVelocity = new Vector3(this.rb.linearVelocity.x, this.jumpStrength, this.rb.linearVelocity.z);
      }
    }

    // Direction for enemy to face
    Quaternion lookRotation = this.lookRotation(acceleration);

    // Rotation
    this.enemy.transform.rotation = Quaternion.RotateTowards(this.enemy.transform.rotation, lookRotation, 2f);
    this.rb.angularVelocity *= 0.75f;
  }
  protected void wander(){
    // Move for when enemy doesn't know player location
    this.setWanderCenter();
    Vector3 noiseRotation = Quaternion.Euler(0f, Mathf.PerlinNoise(Time.time * 0.5f, this.noiseSeed) * 360f + this.timeDelay * 360f, 0f) * new Vector3(1f, 0f, 0f);

    // Don't wander too far from start
    Vector3 toCenter = this.lastWanderCenter - this.enemy.transform.position;
    toCenter = new Vector3(toCenter.x, 0f, toCenter.z);
    float toCenterStrength = Mathf.Max(0f, this.sigmoid(0.2f * new Vector3(toCenter.x, 0f, toCenter.z).magnitude - 4.5f) - 0.015f);

    this.enemy.transform.rotation = this.lookRotation(noiseRotation * (1f - toCenterStrength) + toCenter * toCenterStrength);

    this.rb.linearVelocity = this.enemy.transform.forward * this.wanderSpeed + new Vector3(0.75f * this.rb.linearVelocity.x, this.rb.linearVelocity.y, 0.75f * this.rb.linearVelocity.z);

    this.stopAtHeightDifference();
  }
  protected virtual void stopAtHeightDifference(){
    // Don't fall into cliffs / move into walls while wandering (still possible (but less likely)
    float terrainHeight = this.getHeightInFront(this.rb.linearVelocity);
    float heightDifference = terrainHeight - (this.enemy.transform.position.y - 1f);

    if(Mathf.Abs(heightDifference) > this.maxJumpHeight){
      this.rb.linearVelocity *= 0.06f;
    }else if(heightDifference > 0.1f){
      this.rb.linearVelocity = new Vector3(this.rb.linearVelocity.x, this.jumpStrength, this.rb.linearVelocity.z);
    }
  }
  protected void setWanderCenter(){
    if(this.setNewWanderCenter){
      this.lastWanderCenter = this.enemy.transform.position;
      this.setNewWanderCenter = false;
    }
  }
  protected virtual void attack(){
    this.spinWeapons();

    if(this.reloadTimer < this.reloadTime || Mathf.Pow(this.enemy.transform.position.x - Enemy.player.transform.position.x, 2) + Mathf.Pow(this.enemy.transform.position.z - Enemy.player.transform.position.z, 2) > this.range * this.range){
      this.reloadTimer += Time.deltaTime;
      return;
    }
    this.reloadTimer = 0f;

    // Alternate between guns or shoot from all at once
    if(alternateGuns){
      this.fire(this.lastFired);
      this.lastFired = (this.lastFired + 1) % this.gunPositions.Count;
    }else{
      for(int i = 0; i < this.gunPositions.Count; i++){
        this.fire(i);
      }
    }
  }
  protected void fire(int i){
    // Where bullet is fired from
    Vector3 fireLocation = this.gunPositions[i].position + this.rb.linearVelocity * Time.deltaTime;
    Quaternion lookRotation;
    if(this.overrideDirection){
      lookRotation = this.firingDirectionOverride;
    }else{
      // fire forward
      lookRotation = Quaternion.LookRotation(Enemy.player.transform.position - fireLocation);
      // rotate firing angle towards player (max firingFreedom degrees)
      lookRotation = Quaternion.RotateTowards(this.enemy.transform.rotation, lookRotation, this.firingFreedom);
    }
    // rotate firing angle randomly (constrained by accuracy degrees)
    lookRotation = Quaternion.RotateTowards(lookRotation, Random.rotation, this.accuracy);

    // Fire laser / missile (through EnemyHub)
    if(this.homing){
      Enemy.enemyHub.shootMissile(fireLocation, this.shotSpeed * (lookRotation * new Vector3(0f, 0f, 0.45f)), this.damage, this.homingStartFrame, this.maxHomingFrames, this.homingStrength); 
    }else{
      Enemy.enemyHub.shoot(fireLocation, this.shotSpeed * (lookRotation * new Vector3(0f, 0f, 0.45f)), this.damage);
    }
  }
  protected void spinWeapons(){
    if(this.spinMode == 0 || (this.spinMode & Enemy.spinInPlace) > 0){
      return;
    }
    for(int i = 0; i < this.gunPositions.Count; i++){
      float offsetAngle = i * 2f * Mathf.PI / this.gunPositions.Count;
      float weaponSpinSpeed = this.weaponSpinSpeed;
      float gunWobbleDistance = this.gunWobbleDistance;

      // If Spinning direction alternates
      if((this.spinMode & Enemy.spinAlternate) > 0){
        weaponSpinSpeed = Mathf.Abs(weaponSpinSpeed);
        if(i % 2 == 0){
          weaponSpinSpeed *= -1f;
          gunWobbleDistance *= -1f;
        }else{
        }
        if(this.gunPositions.Count == 2){
          offsetAngle = Mathf.PI / 2f;
        }else if(this.gunPositions.Count == 4){
          offsetAngle += Mathf.PI / 2f;
        }
      }
      
      // Trig calculations for spin
      float s = this.gunPositionDistance * Mathf.Sin(weaponSpinSpeed * (Time.time + this.timeDelay) + offsetAngle);
      float c = this.gunPositionDistance * Mathf.Cos(weaponSpinSpeed * (Time.time + this.timeDelay) + offsetAngle);
      float x = this.gunPositions[i].transform.localPosition.x;
      float y = this.gunPositions[i].transform.localPosition.y;
      float z = this.gunPositions[i].transform.localPosition.z;
      
      // Add wobble
      if((this.spinMode & Enemy.spinWobble) > 0){
        x = y = z = Mathf.Sin(weaponSpinSpeed * (Time.time + this.timeDelay) + offsetAngle) * gunWobbleDistance;
      }

      // Apply spin
      x = (this.spinMode & Enemy.spinX) > 0 ? s : x;
      y = (this.spinMode & Enemy.spinY) > 0 ? ((this.spinMode & Enemy.spinX) > 0 ? c : s) : y;
      z = (this.spinMode & Enemy.spinZ) > 0 ? c : z;
      this.gunPositions[i].transform.localPosition = new Vector3(x, y, z);
    }
  }
  public void takeDamage(float damage){
    this.timeToForget = Mathf.Max(this.maxMemoryAttacked, this.timeToForget);
    Enemy.enemyHub.relayHiveMessage(this.hiveMemberID, "shot-at");

    this.health -= damage;
    // Debug.Log("Health: " + this.health);
    if(this.health <= 0f){
      Enemy.enemyHub.enemyDied(this, this.enemy);
    }
  }
  public Vector3 getPosition(){
    return this.enemy.transform.position;
  }
  public void Update(){
    if(this.knowsPlayerLocation){
      this.move();
      this.setNewWanderCenter = true;
    }else{
      this.wander();
    }
    if((this.knowsPlayerLocation || this.alwaysAttack) && this.canShoot){
      this.attack();
    }
    this.updateStayedStillCount();
    this.searchForPlayer();
    this.frameCount++;
  }
  protected Quaternion lookRotation(Vector3 lookAngle){
    // Regular Quaternion.LookRotation but returns enemy rotation if lookAngle is zero
    if(lookAngle == Vector3.zero){
      return this.enemy.transform.rotation;
    }else{
      return Quaternion.LookRotation(lookAngle);
    }
  }
  protected void searchForPlayer(){
    Vector2 enemyPosition = new Vector2(this.enemy.transform.position.x, this.enemy.transform.position.z);
    Vector2 playerPosition = new Vector2(Enemy.player.transform.position.x, Enemy.player.transform.position.z);

    float distance = Vector2.Distance(enemyPosition, playerPosition);
    float angleToPlayer = Vector2.Angle(new Vector2(this.enemy.transform.forward.x, this.enemy.transform.forward.z), playerPosition - enemyPosition);

    this.timeToForget -= Time.deltaTime;
    this.knowsPlayerLocation = this.timeToForget > 0f;

    if(distance > this.focusedFocalRange
    || (distance > this.focalRange && !this.knowsPlayerLocation)
    || (distance > this.absoluteRange && angleToPlayer > this.focalAngle
       && (!Enemy.enemyHub.isPlayerMakingNoise() || distance > this.hearingRange) 
       && !this.knowsPlayerLocation)
    ){
      this.playerInSight = false;
      this.canShoot = !this.checkIfCanShoot;
      return;
    }

    if((this.frameCount + this.checkOffset) % Enemy.checkInterval == 0){
      this.checkIfPlayerInSight();
    }

    if(this.playerInSight || distance < this.absoluteRange || (Enemy.enemyHub.isPlayerMakingNoise() && distance < this.hearingRange)){
      this.timeToForget = Mathf.Max(this.maxMemoryVision, this.timeToForget);
      Enemy.enemyHub.relayHiveMessage(this.hiveMemberID, "spotted");
    }
  }
  protected void checkIfPlayerInSight(){
    // raycast from enemy head to player head
    RaycastHit hit;
    if(Physics.Linecast(this.enemy.transform.position + this.head, Enemy.player.transform.position + Enemy.playerHead, out hit)){
      if(hit.transform.gameObject == Enemy.player){
        this.playerInSight = true;
      }else{
        this.playerInSight = false;
      }
    }

    // raycast from enemy gun to player body (check if in scope)
    this.canShoot = false;
    if(this.playerInSight && Vector3.Distance(this.enemy.transform.position, Enemy.player.transform.position) < this.range && this.checkIfCanShoot){
      this.canShoot = true;
      // Check only one gun by default
      for(int i = 0; i < (this.checkAllGuns ? this.gunPositions.Count : 1); i++){
        if(Physics.Linecast(this.gunPositions[i].position, Enemy.player.transform.position, out hit)){
          if(hit.transform.gameObject != Enemy.player){
            this.canShoot = false;
            return;
          }
        }
      }
    }else if(!this.checkIfCanShoot){
      this.canShoot = true;
    }
  }
  public int getHiveMemberID(){
    return this.hiveMemberID;
  }
  public void hiveMemberShotAt(){
    this.timeToForget = Mathf.Max(this.maxMemoryAttacked, this.timeToForget);
  }
  public void hiveMemberSpotted(){
    this.timeToForget = Mathf.Max(this.maxMemoryVision, this.timeToForget);
  }

  static public Enemy createEnemy(Vector3 position, string type = "melee", float strengthScaling = 1f, int hiveMemberID = -1){
    // Sort enemy by type and create new instance of appropriate class
    if(Enemy.isFlying(type)){
      if(Enemy.isMelee(type)){
        return new FlyingMeleeEnemy(position, type, strengthScaling, hiveMemberID);
      }
      return new FlyingEnemy(position, type, strengthScaling, hiveMemberID);
    }
    if(Enemy.isMelee(type)){
      return new MeleeEnemy(position, type, strengthScaling, hiveMemberID);
    }
    return new Enemy(position, type, strengthScaling, hiveMemberID);
  }
  static public bool isFlying(string type){
    if(type == "flying" || type == "flying-double" || type == "flying-melee" || type == "flying-melee-quad" || type == "flying-ufo" || type == "flying-sniper" || type == "flying-missile" || type == "flying-pyramid"){
      return true;
    }
    return false;
  }
  static public bool isMelee(string type){
    if(type == "flying-melee" || type == "flying-melee-quad" || type == "flying-pyramid" || type == "melee-figure-eight" || type == "melee-figure-eight-double" || type == "melee" || type == "melee-egg-beater"){
      return true;
    }
    return false;
  }
  static public void setStaticValues(GameObject player, EnemyHub enemyHub){
    // Set references to other scripts / GameObjects
    Enemy.player = player;
    Enemy.enemyHub = enemyHub;
  }
}
