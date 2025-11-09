using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sword : Laser {
  protected Transform parentTransform;
  protected float range;

  public Sword(GameObject lineRenderer, Transform parentTransform, float range, float damage = 1f) : base(lineRenderer, parentTransform.position, parentTransform.localPosition, damage){
    this.gameObject.transform.parent = parentTransform;
    this.parentTransform = parentTransform;
    this.range = range;
    this.trail.Add(this.parentTransform.position);

    this.lineRenderer.startWidth = 0.08f;
    this.lineRenderer.endWidth = 0.05f;
  }
  protected override void move(){
    if(this.trail.Count == 0){
      return;
    }else{
      this.trail[0] = this.parentTransform.position;
    }
    this.position = this.parentTransform.position;
    this.direction = (this.parentTransform.position - this.parentTransform.parent.position).normalized * this.range;
    if(this.hit()) return;

    this.position += this.direction;
  }
  public override bool run(){
    this.move();
    this.draw();

    return false;
  }
}
