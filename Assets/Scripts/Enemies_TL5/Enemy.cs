using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy{
  protected static EnemyHub enemyHub;
  protected GameObject enemy;
  protected EnemyController enemyController;
  protected Rigidbody rb;
  protected float maxJumpHeight = 1.6f;
  protected int stayedStillCount = 0;
  protected int frameCount = 0;

  protected float health = 100;
  protected float maxHealth = 100;
  protected float damage = 1f;

  protected float reloadTimer = 0f;
  protected float reloadTime  = 1f;
  protected float range = 20f;
  protected bool alternateGuns = true;
  protected int lastFired = 0;
  protected float movementSpeed = 0.8f;

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

    switch(type){
    }

    this.applyStrengthScaling(strengthScaling);
    Enemy.enemyHub.addEnemy(this);
  }
  protected void applyStrengthScaling(float strengthScaling){
    this.maxHealth *= strengthScaling;
    this.health = this.maxHealth;
    this.damage *= (strengthScaling + 1f) / 2f;
    this.movementSpeed *= Mathf.Log(strengthScaling) + 1f;
    this.reloadTime /= (Mathf.Log(strengthScaling) + 3f) / 3f;
  }
  public void updateStayedStillCount(){
    if(Mathf.Abs(this.rb.linearVelocity.y) < 0.05f){
      this.stayedStillCount++;
    }else{
      this.stayedStillCount = 0;
    }
  }
  protected bool isOnGround(){
    float terrainHeight = Enemy.enemyHub.getHeight(new Vector2(this.enemy.transform.position.x, this.enemy.transform.position.z));
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
    Vector3 acceleration = (toPlayerPosition - this.enemy.transform.position).normalized * 1f;
    acceleration = new Vector3(acceleration.x, 0f, acceleration.z);

    if(isOnGround() || !obstacleInFront(acceleration, 0f)){  
      this.rb.linearVelocity = new Vector3(0.75f * this.rb.linearVelocity.x, this.rb.linearVelocity.y, 0.75f * this.rb.linearVelocity.z) + acceleration;
    }

    if(isOnGround() && obstacleInFront(acceleration)){
      this.rb.linearVelocity = new Vector3(this.rb.linearVelocity.x, 7f, this.rb.linearVelocity.z);
    }

    //this.rb.MovePosition(this.rb.position + this.velocity);
    //this.rb.linearVelocity = new Vector3(5f, rb.velocity.y, 0f);
    //this.enemy.transform.position += this.velocity;
  }
  protected virtual void attack(){
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
    Quaternion lookRotation = Quaternion.LookRotation(Enemy.enemyHub.getPlayerPosition() - fireLocation);
    lookRotation = Quaternion.RotateTowards(this.enemy.transform.rotation, lookRotation, 5f);
    lookRotation = Quaternion.RotateTowards(lookRotation, Random.rotation, 3f);
    Enemy.enemyHub.shoot(fireLocation, lookRotation * new Vector3(0f, 0f, 0.45f));
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
    this.attack();
    this.updateStayedStillCount();
    this.frameCount++;
  }

  static public Enemy createEnemy(Vector3 position, string type = "basic", float strengthScaling = 1f, int hiveMemberID = -1){
    if(Enemy.isFlying(type)){
      if(Enemy.isMelee(type)){
        return new FlyingMeleeEnemy(position, type, strengthScaling, hiveMemberID);
      }else{
        return new FlyingEnemy(position, type, strengthScaling, hiveMemberID);
      }
    }
    return new Enemy(position, type, strengthScaling, hiveMemberID);
  }
  static public bool isFlying(string type){
    if(type == "flying" || type == "flying-double" || type == "flying-melee" || type == "flying-melee-quad"){
      return true;
    }
    return false;
  }
  static public bool isMelee(string type){
    if(type == "flying-melee" || type == "flying-melee-quad"){
      return true;
    }
    return false;
  }
  static public void setStaticValues(EnemyHub enemyHub){
    Enemy.enemyHub = enemyHub;
  }
}
