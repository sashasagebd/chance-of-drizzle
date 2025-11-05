using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy{
  protected GameObject enemy;
  protected EnemyController enemyController;
  protected EnemyHub enemyHub;
  protected Rigidbody rb;
  protected float maxJumpHeight = 1.6f;
  protected int stayedStillCount = 0;
  protected int frameCount = 0;

  protected float health = 100;
  protected float maxHealth = 100;

  //protected Vector3 velocity;
  public Enemy(EnemyHub enemyHub, Vector3 position, string type, float strengthScaling, int hiveMemberID){
    this.enemy = enemyHub.createEnemyGameObject();
    this.enemy.transform.position = position;
    this.enemyHub = enemyHub;
    this.enemyController = this.enemy.GetComponent<EnemyController>();
    this.enemyController.enemy = this;
    this.rb = this.enemy.GetComponent<Rigidbody>();
    //this.rb.isKinematic = true;

    enemyHub.addEnemy(this);
  }
  public void updateStayedStillCount(){
    if(Mathf.Abs(this.rb.linearVelocity.y) < 0.05f){
      this.stayedStillCount++;
    }else{
      this.stayedStillCount = 0;
    }
  }
  protected bool isOnGround(){
    float terrainHeight = this.enemyHub.getHeight(new Vector2(this.enemy.transform.position.x, this.enemy.transform.position.z));
    if(Mathf.Abs(terrainHeight - (this.enemy.transform.position.y - 1f)) < 0.05f || stayedStillCount > 7){
      return true;
    }
    return false;
  }
  protected float getHeightInFront(Vector3 moveDir){
    Vector3 forward = new Vector3(moveDir.x, 0f, moveDir.z).normalized * 0.55f + this.enemy.transform.position;
    return this.enemyHub.getHeight(new Vector2(forward.x, forward.z));
  }
  protected bool obstacleInFront(Vector3 moveDir, float strictness = 0.2f){
    float terrainHeight = this.getHeightInFront(moveDir);
    if(terrainHeight - (this.enemy.transform.position.y - 1f) > strictness){
    //&& terrainHeight - (this.enemy.transform.position.y - 1f) < this.maxJumpHeight){
      return true;
    }
    return false;
  }
  /*
  // Without virtual keyword (protected -> private)
  private void move(){
  /*/
  // Use virtual keyword to make this method overridable
  protected virtual void move(){
  //*/
    Vector3 toPlayerPosition = this.enemyHub.EnemyPathToPlayer(this.enemy.transform.position);
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
  public void takeDamage(float damage){
    this.health -= damage;
    Debug.Log("Health: " + this.health);
    if(this.health <= 0f){
      this.enemyHub.enemyDied(this, this.enemy);
    }
  }
  public void Update(){
    this.move();
    this.updateStayedStillCount();
    this.frameCount++;
  }

  static public Enemy createEnemy(EnemyHub enemyHub, Vector3 position, string type = "basic", float strengthScaling = 1f, int hiveMemberID = -1){
    if(Enemy.isFlying(type)){
      return new FlyingEnemy(enemyHub, position, type, strengthScaling, hiveMemberID);
    }
    return new Enemy(enemyHub, position, type, strengthScaling, hiveMemberID);
  }
  static public bool isFlying(string type){
    if(type == "flying"){
      return true;
    }
    return false;
  }
}
