using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyingEnemy : Enemy {
  protected float hoverHeight = 3f;
  protected float circleRadius;
  protected float circlingSpeed = 0.7f;
  protected float hoverBobbingAmplitude = 0.25f;
  protected float hoverBobbingSpeed = 0.3f;
  protected float timeDelay = 0f;
  protected float movementSpeed = 0.8f;
  protected bool lookAtPlayer = true;

  public FlyingEnemy(Vector3 position, string type, float strengthScaling, int hiveMemberID) : base(position, type, strengthScaling, hiveMemberID){
    this.timeDelay = Random.Range(0f, 10f);
    this.circlingSpeed = (Random.Range(0f, 1f) < 0.5f ? this.circlingSpeed : -this.circlingSpeed) * (Random.Range(0.85f, 1.15f));
    this.circleRadius = 3f + Random.Range(0, 2.5f);

    this.rb.useGravity = false;

    CapsuleCollider capsuleCollider = this.enemy.GetComponent<CapsuleCollider>();
    capsuleCollider.radius = 0.45f;
    capsuleCollider.height = 0f;

    MeshRenderer meshRenderer = this.enemy.GetComponent<MeshRenderer>();
    meshRenderer.enabled = false;

    switch(type){
      case "flying":
        this.circleRadius *= 1.3f;
        this.reloadTime = 1.5f;
      break;
      case "flying-double":
        this.movementSpeed = 0.75f;
        this.circlingSpeed *= 0.75f;
        this.reloadTime = 0.5f;
      break;
    }
    this.range = this.circleRadius * 1.3f + 1.5f;
  }
  protected virtual Vector3 moveClose(ref Vector3 toPlayerPosition, float hoverHeightCurrent){
    toPlayerPosition += new Vector3(this.circleRadius * Mathf.Sin(this.circlingSpeed * Time.time + this.timeDelay), 0f, this.circleRadius * Mathf.Cos(this.circlingSpeed * Time.time + this.timeDelay));
    return toPlayerPosition - this.enemy.transform.position;
  }
  protected override void move(){
    float hoverHeightCurrent = this.hoverHeight * (1f + this.hoverBobbingAmplitude * Mathf.Sin(this.hoverBobbingSpeed * Time.time + this.timeDelay));
    Vector3 toPlayerPosition = Enemy.enemyHub.getPlayerPosition();
    Quaternion lookRotation = Quaternion.LookRotation(toPlayerPosition + new Vector3(0f, 0.8f, 0f) - this.enemy.transform.position);

    toPlayerPosition += new Vector3(0f, hoverHeightCurrent, 0f);
    Vector3 acceleration = toPlayerPosition - this.enemy.transform.position;

    float dist = Mathf.Pow(acceleration.x, 2) + Mathf.Pow(acceleration.z, 2);
    if(dist < Mathf.Pow(this.circleRadius + 0.5f, 2)){
      acceleration = moveClose(ref toPlayerPosition, hoverHeightCurrent);
      if(!this.lookAtPlayer){
        lookRotation = Quaternion.LookRotation(this.rb.linearVelocity);
      }
    }else{
      acceleration = acceleration.normalized;
      acceleration = new Vector3(acceleration.x, 1.0f + hoverHeightCurrent + this.getHeightInFront(acceleration) - this.enemy.transform.position.y, acceleration.z);
      if(dist > Mathf.Pow(this.range + 1.0f, 2)){
        lookRotation = Quaternion.LookRotation(this.rb.linearVelocity);
      }
    }
    acceleration = acceleration.normalized;

    this.rb.linearVelocity *= 0.75f;
    this.rb.linearVelocity += acceleration * this.movementSpeed;

    this.enemy.transform.rotation = Quaternion.RotateTowards(this.enemy.transform.rotation, lookRotation, 2f);
    this.rb.angularVelocity *= 0.75f;
  }
}
