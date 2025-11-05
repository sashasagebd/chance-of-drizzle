using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy{
  private GameObject enemy;
  private EnemyController enemyController;
  private EnemyHub enemyHub;
  private Rigidbody rb;
  private float maxJumpHeight = 1.6f;
  private int stayedStillCount = 0;

  private float health = 100;
  private float maxHealth = 100;

  //private Vector3 velocity;
  public Enemy(GameObject enemy, EnemyHub enemyHub){
    this.enemy = enemy;
    this.enemyHub = enemyHub;
    this.enemyController = this.enemy.GetComponent<EnemyController>();
    this.enemyController.enemy = this;
    this.rb = this.enemy.GetComponent<Rigidbody>();
    //this.rb.isKinematic = true;
  }
  public void updateStayedStillCount(){
    if(Mathf.Abs(this.rb.linearVelocity.y) < 0.05f){
      this.stayedStillCount++;
    }else{
      this.stayedStillCount = 0;
    }
  }
  private bool isOnGround(){
    float terrainHeight = this.enemyHub.getHeight(new Vector2(this.enemy.transform.position.x, this.enemy.transform.position.z));
    if(Mathf.Abs(terrainHeight - (this.enemy.transform.position.y - 1f)) < 0.05f || stayedStillCount > 7){
      return true;
    }
    return false;
  }
  private bool obstacleInFront(Vector3 moveDir, float strictness = 0.2f){
    Vector3 forward = moveDir.normalized * 0.55f + this.enemy.transform.position;
    float terrainHeight = this.enemyHub.getHeight(new Vector2(forward.x, forward.z));
    if(terrainHeight - (this.enemy.transform.position.y - 1f) > strictness){
    //&& terrainHeight - (this.enemy.transform.position.y - 1f) < this.maxJumpHeight){
      return true;
    }
    return false;
  }
  private void move(){
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
  }
}
