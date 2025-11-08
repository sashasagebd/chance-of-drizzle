using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyingMeleeEnemy : FlyingEnemy{
  protected List<Sword> swords = new List<Sword>();
  protected float weaponSpinSpeed = 7f;
  protected float swordRange = 1.95f;
  protected float gunPositionDistance;
  protected float circlingSpeedSlow = 0.5f;

  public FlyingMeleeEnemy(Vector3 position, string type, float strengthScaling, int hiveMemberID) : base(position, type, strengthScaling, hiveMemberID){
    // Always spin the same way to avoid hitting gun
    this.circlingSpeed = -Mathf.Abs(this.circlingSpeed);
    this.weaponSpinSpeed = (Random.Range(0f, 1f) < 0.5f ? this.weaponSpinSpeed : -this.weaponSpinSpeed) * (Random.Range(0.7f, 1.2f));
    this.circlingSpeedSlow = (Random.Range(0f, 1f) < 0.5f ? this.circlingSpeed : -this.circlingSpeed) * (Random.Range(0.2f, 1f));
    this.movementSpeed *= 1.6f;
    this.circlingSpeed *= 1.4f;

    switch(type){
      case "flying":
        this.circleRadius = 6f;
      break;
    }
    this.range = this.circleRadius * 1.3f + 1.5f;

    this.gunPositionDistance = Mathf.Pow(Mathf.Pow(this.gunPositions[0].localPosition.x, 2) + Mathf.Pow(this.gunPositions[0].localPosition.y, 2), 0.5f);
    for(int i = 0; i < this.gunPositions.Count; i++){
      this.swords.Add(Enemy.enemyHub.sword(this.gunPositions[i], this.swordRange));
    }
  }
  protected override Vector3 moveClose(ref Vector3 toPlayerPosition, float hoverHeightCurrent){
    float s = Mathf.Sin(this.circlingSpeed * Time.time + this.timeDelay);
    float c = Mathf.Cos(this.circlingSpeed * Time.time + this.timeDelay);
    Vector3 goTo = Quaternion.Euler(0f, this.circlingSpeedSlow * Time.time + this.timeDelay, 0f) * new Vector3(0.38f * this.swordRange * s, 0f, this.circleRadius * c);
    float p = 0.3f - 0.5f * Mathf.Cos(2f * (this.circlingSpeed * Time.time + this.timeDelay));
    toPlayerPosition += goTo - new Vector3(0f, p * hoverHeightCurrent, 0f);
    return toPlayerPosition - this.enemy.transform.position;
  }
  protected override void attack(){
    /*
    if(Mathf.Pow(this.enemy.transform.position.x - Enemy.enemyHub.getPlayerPosition().x, 2) + Mathf.Pow(this.enemy.transform.position.z - Enemy.enemyHub.getPlayerPosition().z, 2) > this.range * this.range){
      return;
    }
    */
    for(int i = 0; i < this.gunPositions.Count; i++){
      float offsetAngle = i * 2f * Mathf.PI / this.gunPositions.Count;
      float s = this.gunPositionDistance * Mathf.Sin(this.weaponSpinSpeed * Time.time + this.timeDelay + offsetAngle);
      float c = this.gunPositionDistance * Mathf.Cos(this.weaponSpinSpeed * Time.time + this.timeDelay + offsetAngle);
      this.gunPositions[i].transform.localPosition = new Vector3(s, c, 0f);
      this.swords[i].run();
    }
  }
}
