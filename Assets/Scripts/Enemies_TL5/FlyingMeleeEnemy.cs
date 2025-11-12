using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyingMeleeEnemy : FlyingEnemy{
  protected List<Sword> swords = new List<Sword>();
  protected float swordRange = 1.95f;
  protected float circlingSpeedSlow = 0.5f;

  public FlyingMeleeEnemy(Vector3 position, string type, float strengthScaling, int hiveMemberID)
  : base(position, type, strengthScaling, hiveMemberID){

    // Always spin the same way to avoid hitting gun (counter clockwise)
    this.circlingSpeed = -Mathf.Abs(this.circlingSpeed);
    this.circlingSpeedSlow = (Random.Range(0f, 1f) < 0.5f ? this.circlingSpeed : -this.circlingSpeed) * (Random.Range(0.2f, 1f));
    this.checkIfCanShoot = false;

    // Update swords regardless of distance from player
    this.alwaysAttack = true;

    // Individual stats for various flying enemy types
    switch(type){
      case "flying-melee":
        this.circleRadius = 5f;
        this.movementSpeed = 1.4f;
        this.circlingSpeed *= 1.4f;
        this.damage = 1f;
        this.maxHealth = 65f;
        this.spinMode = Enemy.spinX | Enemy.spinY;
      break;
      case "flying-melee-quad":
        this.swordRange = 1.2f;
        this.circleRadius = 0.5f;
        this.hoverHeight = 0.8f;
        this.movementSpeed = 0.3f;
        this.weaponSpinSpeed *= 0.4f;
        this.damage = 3.6f;
        this.maxHealth = 115f;
        this.spinMode = Enemy.spinX | Enemy.spinY;
      break;
      case "flying-pyramid":
        this.damage = 5.5f;
        this.maxHealth = 125f;
        this.movementSpeed = 0f;
        this.spinMode = Enemy.spinX | Enemy.spinY;
        this.swordRange = 300f;
        this.lookIntoSpace = true;
        this.weaponSpinSpeed = 1.5f;
        this.enemy.transform.position += new Vector3(0f, 6f + this.getTerrainHeight() - this.enemy.transform.position.y, 0f);
      break;
    }
    // Set attack range to larger than circling radius (attack range is when enemy gets into attack pattern, not actual attack range)
    this.range = this.circleRadius * 1.3f + 1.5f;

    // Run setup functions
    this.applyStrengthScaling(strengthScaling);
    this.setGunPositionDistance();
    this.setFindRanges();

    // Create line renderers for the swords
    for(int i = 0; i < this.gunPositions.Count; i++){
      this.swords.Add(Enemy.enemyHub.sword(this.gunPositions[i], this.swordRange, this.damage));
    }
  }
  protected override Vector3 moveClose(ref Vector3 toPlayerPosition, float hoverHeightCurrent){
    // When close, spin in narrow ellipses around player (almost touching player at 2 points)
    float s = Mathf.Sin(this.circlingSpeed * Time.time + this.timeDelay);
    float c = Mathf.Cos(this.circlingSpeed * Time.time + this.timeDelay);
    Vector3 goTo = Quaternion.Euler(0f, this.circlingSpeedSlow * Time.time + this.timeDelay, 0f) * new Vector3(0.38f * this.swordRange * s, 0f, this.circleRadius * c);
    float p = 0.3f - 0.5f * Mathf.Cos(2f * (this.circlingSpeed * Time.time + this.timeDelay));
    toPlayerPosition += goTo - new Vector3(0f, p * hoverHeightCurrent, 0f);
    return toPlayerPosition - this.enemy.transform.position;
  }
  protected override void attack(){
    // Spin and update swords
    this.spinWeapons();
    for(int i = 0; i < this.gunPositions.Count; i++){
      this.swords[i].run();
    }
  }
}
