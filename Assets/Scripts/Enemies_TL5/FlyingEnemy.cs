using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyingEnemy : Enemy {
  protected float hoverHeight = 3f;
  protected float attackPrepareRange = 6f;
  protected float circleRadius;
  protected float circlingSpeed = 0.9f;
  protected float hoverBobbingAmplitude = 0.25f;
  protected float hoverBobbingSpeed = 0.7f;
  protected float timeDelay = 0f;
  protected float movementSpeed = 1f;

  public FlyingEnemy(EnemyHub enemyHub, Vector3 position, string type, float strengthScaling, int hiveMemberID) : base(enemyHub, position, type, strengthScaling, hiveMemberID){
    this.timeDelay = Random.Range(0f, 10f);
    this.circlingSpeed = (Random.Range(0f, 1f) < 0.5f ? this.circlingSpeed : -this.circlingSpeed) * (Random.Range(0.85f, 1.15f));
    this.circleRadius = Random.Range(attackPrepareRange - 2.3f, attackPrepareRange - 0.5f);

    this.rb.useGravity = false;

    CapsuleCollider capsuleCollider = this.enemy.GetComponent<CapsuleCollider>();
    capsuleCollider.radius = 0.45f;
    capsuleCollider.height = 0f;

    MeshRenderer meshRenderer = this.enemy.GetComponent<MeshRenderer>();
    meshRenderer.enabled = false;

    Transform objT;
    GameObject obj;

    switch(type){
      case "flying":
        objT = this.enemy.transform.Find("flyingEnemy1");
        obj = objT.gameObject;
        obj.SetActive(true);
      break;
      case "flying-double":
        objT = this.enemy.transform.Find("flyingEnemy2");
        obj = objT.gameObject;
        obj.SetActive(true);

        this.movementSpeed = 0.75f;
        this.circlingSpeed *= 0.75f;
      break;
    }
  }
  
  /*
  // Without virtual keyword (protected -> private)
  private void move(){
  /*/
  // Use overrideable method that was set with virtual in the parent
  protected override void move(){
  //*/
    float hoverHeightCurrent = this.hoverHeight * (1f + this.hoverBobbingAmplitude * Mathf.Sin(this.hoverBobbingSpeed * Time.time + this.timeDelay));
    Vector3 toPlayerPosition = this.enemyHub.GetPlayerPosition();
    Quaternion lookRotation = Quaternion.LookRotation(toPlayerPosition + new Vector3(0f, 0.8f, 0f) - this.enemy.transform.position);

    toPlayerPosition += new Vector3(0f, hoverHeightCurrent, 0f);
    Vector3 acceleration = toPlayerPosition - this.enemy.transform.position;

    if(Mathf.Pow(acceleration.x, 2) + Mathf.Pow(acceleration.z, 2) < Mathf.Pow(this.attackPrepareRange, 2)){
      toPlayerPosition += new Vector3(this.circleRadius * Mathf.Sin(this.circlingSpeed * Time.time + this.timeDelay), 0f, this.circleRadius * Mathf.Cos(this.circlingSpeed * Time.time + this.timeDelay));
      acceleration = toPlayerPosition - this.enemy.transform.position;
    }else{
      acceleration = acceleration.normalized;
      acceleration = new Vector3(acceleration.x, 1.0f + hoverHeightCurrent + this.getHeightInFront(acceleration) - this.enemy.transform.position.y, acceleration.z);
      lookRotation = Quaternion.LookRotation(this.rb.linearVelocity);
    }
    acceleration = acceleration.normalized;

    this.rb.linearVelocity *= 0.75f;
    this.rb.linearVelocity += acceleration * this.movementSpeed;

    this.enemy.transform.rotation = Quaternion.RotateTowards(this.enemy.transform.rotation, lookRotation, 2.5f);
    this.rb.angularVelocity *= 0.75f;
  }
}
