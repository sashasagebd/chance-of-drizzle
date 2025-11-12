using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sword : Laser {
  // Unity Integration
  protected Transform parentTransform;

  // Sword parameters
  protected float range;
  protected Vector3 overrideDirection = Vector3.zero;

  public Sword(GameObject lineRenderer, Transform parentTransform, float range, float damage = 1f)
  : base(lineRenderer, parentTransform.position, parentTransform.localPosition, damage){

    // Set up Unity integration
    this.gameObject.transform.parent = parentTransform;
    this.parentTransform = parentTransform;

    // Set up sword parameters
    this.range = range;
    this.trail.Add(this.parentTransform.position);
    
    // Change line renderer parameters
    this.lineRenderer.startWidth = 0.08f;
    this.lineRenderer.endWidth = 0.05f;
  }
  protected override void move(){
    if(this.trail.Count == 0){
      return;
    }else{
      this.trail[0] = this.parentTransform.position;
    }

    // Stay grounded to parent transform, but rotate with it
    this.position = this.parentTransform.position;
    this.direction = (this.overrideDirection == Vector3.zero ? (this.parentTransform.position - this.parentTransform.parent.position).normalized : this.overrideDirection) * this.range;

    // Return if hit so sword stops at point it hit instead of piercing object
    if(this.hit()) return;

    this.position += this.direction;
  }
  public override bool run(){
    // Remove section on death (should never die until Enemy dies)
    this.move();
    this.draw();

    return false;
  }

  // Used for setting additional parameters not set in constructor
  public void setOverrideDirection(Vector3 direction){
    this.overrideDirection = direction;
  }
}
