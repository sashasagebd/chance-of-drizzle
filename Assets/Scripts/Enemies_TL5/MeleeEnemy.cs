using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeEnemy : Enemy {
  protected List<Sword> swords = new List<Sword>();
  protected float swordRange = 1.95f;

  public MeleeEnemy(Vector3 position, string type, float strengthScaling = 1f, int hiveMemberID = -1) : base(position, type, strengthScaling, hiveMemberID){
    switch(type){
      case "melee-figure-eight":
        this.damage = 2.5f;
        this.movementSpeed = 1f;
        this.maxHealth = 170f;
        this.weaponSpinSpeed = 5f;
        this.spinMode = Enemy.spinX | Enemy.spinY | Enemy.spinWobble | Enemy.spinAlternate;
        this.swordRange = 1.7f;
      break;
      case "melee-figure-eight-double":
        this.damage = 1.9f;
        this.movementSpeed = 1f;
        this.maxHealth = 170f;
        this.weaponSpinSpeed = 8f;
        this.spinMode = Enemy.spinX | Enemy.spinY | Enemy.spinWobble | Enemy.spinAlternate;
        this.swordRange = 1.4f;
      break;
    }

    for(int i = 0; i < this.gunPositions.Count; i++){
      this.swords.Add(Enemy.enemyHub.sword(this.gunPositions[i], this.swordRange));
    }

    this.applyStrengthScaling(strengthScaling);
    this.setGunPositionDistance();
  }
  protected override void attack(){
    this.spinWeapons();
    for(int i = 0; i < this.gunPositions.Count; i++){
      this.swords[i].run();
    }
  }
}
