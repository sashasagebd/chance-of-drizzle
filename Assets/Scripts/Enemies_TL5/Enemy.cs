using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy{
  protected static EnemyHub enemyHub;
  protected static GameObject player;

  // Game object / Unity integration
  protected GameObject enemy;
  protected EnemyController enemyController;
  protected Rigidbody rb;

  // Used for jumping / calculating if on ground
  protected float maxJumpHeight = 1.6f;
  protected int stayedStillCount = 0;
  protected int frameCount = 0;

  // Basic stats
  protected float health = 0f;
  protected float maxHealth = 0f;
  protected float damage = 0f;
  protected float reloadTime  = 1e3f;
  protected float movementSpeed = 0f;

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
    this.enemy = Enemy.enemyHub.createEnemyGameObject();
    this.enemy.transform.position = position;
    this.enemyController = this.enemy.GetComponent<EnemyController>();
    this.enemyController.enemy = this;
    this.rb = this.enemy.GetComponent<Rigidbody>();
    //this.rb.isKinematic = true;

    Transform objT = this.enemy.transform.Find(type);
    GameObject obj = objT.gameObject;
    obj.SetActive(true);
    foreach (Transform gunPosition in objT) {
      this.gunPositions.Add(gunPosition);
    }
    this.weaponSpinSpeed = (Random.Range(0f, 1f) < 0.5f ? this.weaponSpinSpeed : -this.weaponSpinSpeed) * (Random.Range(0.7f, 1.2f));

    this.timeDelay = Random.Range(0f, 10f);
    this.checkOffset = Random.Range(0, Enemy.checkInterval);

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

    this.applyStrengthScaling(strengthScaling);
    this.setGunPositionDistance();
    Enemy.enemyHub.addEnemy(this);
  }
  protected void setGunPositionDistance(){
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
  }
  public void updateStayedStillCount(){
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
  protected bool obstacleInFront(Vector3 moveDir, float strictness = 0.2f){
    float terrainHeight = this.getHeightInFront(moveDir);
    if(terrainHeight - (this.enemy.transform.position.y - 1f) > strictness){
    //&& terrainHeight - (this.enemy.transform.position.y - 1f) < this.maxJumpHeight){
      return true;
    }
    return false;
  }
  protected virtual void move(){
    Vector3 toPlayerPosition = Enemy.enemyHub.EnemyPathToPlayer(this.enemy.transform.position);
    Vector3 acceleration = toPlayerPosition - this.enemy.transform.position;

    Vector3 toPlayer = Enemy.enemyHub.getPlayerPosition() - this.enemy.transform.position;

    if(this.stopWhenInRange && toPlayer.magnitude < this.range && !this.keepMoving && this.canShoot){
      acceleration = new Vector3(toPlayer.x, 0f, toPlayer.z);
      
      
    }else if(this.stopWhenInRange && toPlayer.magnitude < this.range * 0.7f && this.canShoot){
      this.keepMoving = false;
    }else{
      this.keepMoving = true;

      acceleration = acceleration.normalized * this.movementSpeed;
      acceleration = new Vector3(acceleration.x, 0f, acceleration.z);
      
      if(isOnGround() || !obstacleInFront(acceleration, 0f)){  
        this.rb.linearVelocity = new Vector3(0.75f * this.rb.linearVelocity.x, this.rb.linearVelocity.y, 0.75f * this.rb.linearVelocity.z) + acceleration;
      }

      if(isOnGround() && obstacleInFront(acceleration)){
        this.rb.linearVelocity = new Vector3(this.rb.linearVelocity.x, 7f, this.rb.linearVelocity.z);
      }
    }

    Quaternion lookRotation = this.lookRotation(acceleration);

    this.enemy.transform.rotation = Quaternion.RotateTowards(this.enemy.transform.rotation, lookRotation, 2f);
    this.rb.angularVelocity *= 0.75f;
    //this.rb.MovePosition(this.rb.position + this.velocity);
    //this.rb.linearVelocity = new Vector3(5f, rb.velocity.y, 0f);
    //this.enemy.transform.position += this.velocity;
  }
  protected virtual void attack(){
    this.spinWeapons();

    if(this.reloadTimer < this.reloadTime || Mathf.Pow(this.enemy.transform.position.x - Enemy.enemyHub.getPlayerPosition().x, 2) + Mathf.Pow(this.enemy.transform.position.z - Enemy.enemyHub.getPlayerPosition().z, 2) > this.range * this.range){
      this.reloadTimer += Time.deltaTime;
      return;
    }
    this.reloadTimer = 0f;

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
    Vector3 fireLocation = this.gunPositions[i].position + this.rb.linearVelocity * Time.deltaTime;
    Quaternion lookRotation;
    if(this.overrideDirection){
      lookRotation = this.firingDirectionOverride;
    }else{
      lookRotation = Quaternion.LookRotation(Enemy.enemyHub.getPlayerPosition() - fireLocation);
      lookRotation = Quaternion.RotateTowards(this.enemy.transform.rotation, lookRotation, this.firingFreedom);
    }
    lookRotation = Quaternion.RotateTowards(lookRotation, Random.rotation, this.accuracy);
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
      float s = this.gunPositionDistance * Mathf.Sin(weaponSpinSpeed * (Time.time + this.timeDelay) + offsetAngle);
      float c = this.gunPositionDistance * Mathf.Cos(weaponSpinSpeed * (Time.time + this.timeDelay) + offsetAngle);
      float x = this.gunPositions[i].transform.localPosition.x;
      float y = this.gunPositions[i].transform.localPosition.y;
      float z = this.gunPositions[i].transform.localPosition.z;
      if((this.spinMode & Enemy.spinWobble) > 0){
        x = y = z = Mathf.Sin(weaponSpinSpeed * (Time.time + this.timeDelay) + offsetAngle) * gunWobbleDistance;
      }
      x = (this.spinMode & Enemy.spinX) > 0 ? s : x;
      y = (this.spinMode & Enemy.spinY) > 0 ? ((this.spinMode & Enemy.spinX) > 0 ? c : s) : y;
      z = (this.spinMode & Enemy.spinZ) > 0 ? c : z;
      this.gunPositions[i].transform.localPosition = new Vector3(x, y, z);
    }
  }
  public void takeDamage(float damage){
    this.health -= damage;
    Debug.Log("Health: " + this.health);
    if(this.health <= 0f){
      Enemy.enemyHub.enemyDied(this, this.enemy);
    }
  }
  public Vector3 getPosition(){
    return this.enemy.transform.position;
  }
  public void Update(){
    this.move();
    if(this.canShoot){
      this.attack();
    }
    this.updateStayedStillCount();
    if((this.frameCount + this.checkOffset) % Enemy.checkInterval == 0){
      this.checkIfPlayerInSight();
    }
    this.frameCount++;
  }
  protected Quaternion lookRotation(Vector3 lookAngle){
    if(lookAngle == Vector3.zero){
      return this.enemy.transform.rotation;
    }else{
      return Quaternion.LookRotation(lookAngle);
    }
  }
  protected void checkIfPlayerInSight(){
    RaycastHit hit;
    if(Physics.Linecast(this.enemy.transform.position + new Vector3(0f, 0.5f, 0f), Enemy.player.transform.position, out hit)){
      if(hit.transform.gameObject == Enemy.player){
        this.playerInSight = true;
      }else{
        this.playerInSight = false;
      }
    }
    this.canShoot = false;
    if(this.playerInSight && Vector3.Distance(this.enemy.transform.position, Enemy.player.transform.position) < this.range && this.checkIfCanShoot){
      this.canShoot = true;
      for(int i = 0; i < 1; i++){ //this.gunPositions.Count; i++){
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

  static public Enemy createEnemy(Vector3 position, string type = "melee", float strengthScaling = 1f, int hiveMemberID = -1){
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
    Enemy.player = player;
    Enemy.enemyHub = enemyHub;
  }
}
