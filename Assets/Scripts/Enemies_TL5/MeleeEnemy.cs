using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeEnemy : Enemy {
  protected List<Sword> swords = new List<Sword>();
  protected float swordRange = 1.95f;

  public MeleeEnemy(Vector3 position, string type, float strengthScaling = 1f, int hiveMemberID = -1) : base(position, type, strengthScaling, hiveMemberID){
    this.stopWhenInRange = false;
    this.checkIfCanShoot = false;

    switch(type){
      case "melee-figure-eight":
        this.damage = 0.9f;
        this.movementSpeed = 1.1f;
        this.maxHealth = 170f;
        this.weaponSpinSpeed = 5f;
        this.spinMode = Enemy.spinX | Enemy.spinY | Enemy.spinWobble | Enemy.spinAlternate;
        this.swordRange = 1.7f;
      break;
      case "melee-figure-eight-double":
        this.damage = 0.7f;
        this.movementSpeed = 0.9f;
        this.maxHealth = 160f;
        this.weaponSpinSpeed = 8f;
        this.spinMode = Enemy.spinX | Enemy.spinY | Enemy.spinWobble | Enemy.spinAlternate;
        this.swordRange = 1.4f;
      break;
      case "melee":
        this.damage = 1.5f;
        this.movementSpeed = 1.2f;
        this.maxHealth = 130f;
        this.weaponSpinSpeed = 3f;
        this.spinMode = Enemy.spinX | Enemy.spinY;
        this.swordRange = 1.5f;
      break;
      case "melee-egg-beater":
        this.damage = 0.1f;
        this.movementSpeed = 1.15f;
        this.maxHealth = 140f;
        this.weaponSpinSpeed = 5f;
        this.spinMode = Enemy.spinX | Enemy.spinY | Enemy.spinInPlace;
        this.swordRange = 1.7f;
        this.gunInPlaceRadius = 0.4f;
      break;
    }

    this.applyStrengthScaling(strengthScaling);

    for(int i = 0; i < this.gunPositions.Count; i++){
      this.swords.Add(Enemy.enemyHub.sword(this.gunPositions[i], this.swordRange, this.damage));
    }

    this.setGunPositionDistance();
  }
  protected override void attack(){
    this.spinWeapons();
    for(int i = 0; i < this.gunPositions.Count; i++){
      if((this.spinMode & Enemy.spinInPlace) > 0){
        float weaponSpinSpeed = Mathf.Abs(this.weaponSpinSpeed) * (i % 2 == 1 ? 1f : -1f);
        float offsetAngle = i * 2f * Mathf.PI / this.gunPositions.Count;
        float s = Mathf.Sin(weaponSpinSpeed * (Time.time + this.timeDelay) + offsetAngle);
        float c = Mathf.Cos(weaponSpinSpeed * (Time.time + this.timeDelay) + offsetAngle);
        float x = (this.spinMode & Enemy.spinX) > 0 ? s : 0f;
        float y = (this.spinMode & Enemy.spinY) > 0 ? ((this.spinMode & Enemy.spinX) > 0 ? c : s) : 0f;
        float z = (this.spinMode & Enemy.spinZ) > 0 ? c : 0f;
        this.swords[i].setOverrideDirection(this.enemy.transform.forward + (this.enemy.transform.rotation * (new Vector3(x, y, z) * this.gunInPlaceRadius)));
      }
      this.swords[i].run();
    }
  }
}
