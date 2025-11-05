using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy{
  private GameObject enemy;
  private EnemyController enemyController;
  private EnemyHub enemyHub;
  private Rigidbody rb;
  //private Vector3 velocity;
  public Enemy(GameObject enemy, EnemyHub enemyHub){
    this.enemy = enemy;
    this.enemyHub = enemyHub;
    this.enemyController = this.enemy.GetComponent<EnemyController>();
    this.enemyController.enemy = this;
    this.rb = this.enemy.GetComponent<Rigidbody>();
    //this.rb.isKinematic = true;
  }
  public void Update(){
    Vector3 toPlayerPosition = this.enemyHub.EnemyPathToPlayer(this.enemy.transform.position);
    Vector3 acceleration = (toPlayerPosition - this.enemy.transform.position).normalized * 0.7f;
    acceleration = new Vector3(acceleration.x, 0f, acceleration.z);
    
    this.rb.linearVelocity *= 0.75f;
    this.rb.linearVelocity += acceleration;

    //this.rb.MovePosition(this.rb.position + this.velocity);
    //this.rb.linearVelocity = new Vector3(5f, rb.velocity.y, 0f);
    //this.enemy.transform.position += this.velocity;
  }
}
