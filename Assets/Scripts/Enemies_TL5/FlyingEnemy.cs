using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyingEnemy : Enemy {
  protected float hoverHeight = 3f;
  protected float attackPrepareRange = 6f;
  protected float circleRadius = 4f;
  protected float circlingSpeed = 0.9f;
  protected float hoverBobbingAmplitude = 0.35f;
  protected float hoverBobbingSpeed = 0.7f;

  public FlyingEnemy(GameObject enemyInstance, EnemyHub enemyHub, string type) : base(enemyInstance, enemyHub, type) {
    this.rb.useGravity = false;
  }
  
  protected override void move(){
    float hoverHeightCurrent = this.hoverHeight * (1f + this.hoverBobbingAmplitude * Mathf.Sin(this.hoverBobbingSpeed * Time.time));
    Vector3 toPlayerPosition = this.enemyHub.GetPlayerPosition();
    toPlayerPosition += new Vector3(0f, hoverHeightCurrent, 0f);
    Vector3 acceleration = toPlayerPosition - this.enemy.transform.position;

    if(Mathf.Pow(acceleration.x, 2) + Mathf.Pow(acceleration.z, 2) < Mathf.Pow(this.attackPrepareRange, 2)){
      toPlayerPosition += new Vector3(this.circleRadius * Mathf.Sin(this.circlingSpeed * Time.time), 0f, this.circleRadius * Mathf.Cos(this.circlingSpeed * Time.time));
      acceleration = toPlayerPosition - this.enemy.transform.position;
    }else{
      acceleration = acceleration.normalized;
      acceleration = new Vector3(acceleration.x, 1.0f + hoverHeightCurrent + this.getHeightInFront(acceleration) - this.enemy.transform.position.y, acceleration.z);
    }
    acceleration = acceleration.normalized;

    this.rb.linearVelocity *= 0.75f;
    this.rb.linearVelocity += acceleration;
  }
}
