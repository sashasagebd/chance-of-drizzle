using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyingEnemy : Enemy {
  // Flying controls
  protected float hoverHeight = 3f;
  protected float circleRadius;
  protected float circlingSpeed = 0.7f;
  protected float hoverBobbingAmplitude = 0.25f;
  protected float hoverBobbingSpeed = 0.3f;

  // Looking (rotation) behaviour
  protected bool lookAtPlayer = true;
  protected bool lookIntoSpace = false;
  protected bool lookHorizontal = false;

  public FlyingEnemy(Vector3 position, string type, float strengthScaling, int hiveMemberID)
  : base(position, type, strengthScaling, hiveMemberID){
    
    // Randomize common parameters
    this.circlingSpeed = (Random.Range(0f, 1f) < 0.5f ? this.circlingSpeed : -this.circlingSpeed) * (Random.Range(0.85f, 1.15f));
    this.circleRadius = 3f + Random.Range(0, 2.5f);

    // Unity settings
    this.rb.useGravity = false;
    CapsuleCollider capsuleCollider = this.enemy.GetComponent<CapsuleCollider>();
    capsuleCollider.radius = 0.45f;
    capsuleCollider.height = 0f;

    // Individual stats for various flying enemy types
    switch(type){
      case "flying":
        this.movementSpeed = 0.8f;
        this.circleRadius *= 1.3f;
        this.reloadTime = 1.5f;
        this.damage = 4.5f;
        this.maxHealth = 75f;
      break;
      case "flying-double":
        this.movementSpeed = 0.75f;
        this.circlingSpeed *= 0.75f;
        this.reloadTime = 0.5f;
        this.damage = 2.3f;
        this.maxHealth = 45f;
      break;
      case "flying-sniper":
        this.movementSpeed = 0.55f;
        this.circlingSpeed *= 0.25f;
        this.circleRadius *= 3.3f;
        this.circleRadius += 10f;
        this.reloadTime = 8.5f;
        this.damage = 7f;
        this.maxHealth = 65f;
        this.accuracy = 0f;
        this.shotSpeed = 2.9f;
      break;
      case "flying-ufo":
        this.movementSpeed = 0.65f;
        this.circlingSpeed *= 2.5f;
        this.circleRadius = 1.1f;
        this.hoverHeight = 8.0f;
        this.hoverBobbingAmplitude = 0f;
        this.reloadTime = 0.15f;
        this.alternateGuns = false;
        this.damage = 1f;
        this.maxHealth = 45f;
        this.accuracy = 4f;
        this.lookAtPlayer = false;
        this.lookHorizontal = true;
        this.firingFreedom = 180f;
        this.spinMode = Enemy.spinX | Enemy.spinY;
        this.shotSpeed = 1f;
        this.checkIfCanShoot = false;
      break;
      case "flying-missile":
        this.movementSpeed = 0.7f;
        this.circlingSpeed *= 0.6f;
        this.circleRadius *= 2.1f;
        this.circleRadius += 6f;
        this.reloadTime = 0.15f;
        this.damage = 0.9f;
        this.maxHealth = 55f;
        this.firingFreedom = 180f;
        this.accuracy = 70f;
        this.homing = true;
        this.spinMode = Enemy.spinY | Enemy.spinZ;
        this.shotSpeed = 0.4f;
        this.checkIfCanShoot = false;
      break;
    }
    // Make range depend on circling radius (hovering in a circle around player)
    this.range = this.circleRadius * 1.3f + 1.5f;

    // Run setup functions
    this.applyStrengthScaling(strengthScaling);
    this.setGunPositionDistance();
    this.setFindRanges();
  }
  protected virtual Vector3 moveClose(ref Vector3 toPlayerPosition, float hoverHeightCurrent){
    // Spin in circles around player when close (overridden in FlyingMeleeEnemy)
    toPlayerPosition += new Vector3(this.circleRadius * Mathf.Sin(this.circlingSpeed * Time.time + this.timeDelay), 0f, this.circleRadius * Mathf.Cos(this.circlingSpeed * Time.time + this.timeDelay));
    return toPlayerPosition - this.enemy.transform.position;
  }
  protected override void move(){
    // Bob up and down
    float hoverHeightCurrent = this.hoverHeight * (1f + this.hoverBobbingAmplitude * Mathf.Sin(this.hoverBobbingSpeed * Time.time + this.timeDelay));
    
    // Move towards player
    Vector3 toPlayerPosition = Enemy.player.transform.position;
    Quaternion lookRotation = this.lookRotation(toPlayerPosition + Enemy.playerHead - this.enemy.transform.position);

    // Move towards point above player at current height
    toPlayerPosition += new Vector3(0f, hoverHeightCurrent, 0f);
    Vector3 acceleration = toPlayerPosition - this.enemy.transform.position;

    // distance from player ignoring y-axis
    float dist = Mathf.Pow(acceleration.x, 2) + Mathf.Pow(acceleration.z, 2);
    if(dist < Mathf.Pow(this.circleRadius + 0.5f, 2)){
      acceleration = moveClose(ref toPlayerPosition, hoverHeightCurrent);
      if(!this.lookAtPlayer){
        lookRotation = this.lookRotation(this.rb.linearVelocity);
      }
    }else{
      // Move in straight line towards player (ignores obstacles)
      acceleration = acceleration.normalized;
      acceleration = new Vector3(acceleration.x, 1.0f + hoverHeightCurrent + this.getHeightInFront(acceleration) - this.enemy.transform.position.y, acceleration.z);
      
      // Only look away from player when far (look at direction of movement)
      if(dist > Mathf.Pow(this.range + 1.0f, 2)){
        lookRotation = this.lookRotation(this.rb.linearVelocity);
      }
    }

    // Special case rotations
    if(this.lookIntoSpace){
      lookRotation = Quaternion.Euler(0f, 0f, 0f);
    }else if(this.lookHorizontal){
      lookRotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
    }

    // Apply acceleration
    acceleration = acceleration.normalized;
    this.rb.linearVelocity *= 0.75f;
    this.rb.linearVelocity += acceleration * this.movementSpeed;

    // Apply rotation
    this.enemy.transform.rotation = Quaternion.RotateTowards(this.enemy.transform.rotation, lookRotation, 2f);
    this.rb.angularVelocity *= 0.75f;
  }
  protected override void stopAtHeightDifference(){
    // Hover over obstacles instead of stopping (called in wander)

    float terrainHeight = this.getHeightInFront(this.rb.linearVelocity);
    float hoverHeightCurrent = this.hoverHeight + this.hoverBobbingAmplitude + 3f;
    this.rb.linearVelocity = new Vector3(this.rb.linearVelocity.x, 0.75f * this.rb.linearVelocity.y, this.rb.linearVelocity.z);
    this.rb.linearVelocity += this.wanderSpeed * 0.8f * new Vector3(0f, (hoverHeightCurrent + terrainHeight) - this.enemy.transform.position.y, 0f);
  }
}
