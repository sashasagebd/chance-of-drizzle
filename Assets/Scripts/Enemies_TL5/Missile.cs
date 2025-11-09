using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Missile : Laser{
  protected int frames = 0;
  protected int homingStartFrame = 6;
  protected int maxHomingFrames = 60;
  protected float movementSpeed = 1f;

  public Missile(GameObject lineRenderer, Vector3 position, Vector3 direction, float damage = 1f) : base(lineRenderer, position, direction, damage){
    this.lineRenderer.startWidth = 0.08f;
    this.lineRenderer.endWidth = 0.05f;

    this.nonStraightPath = true;
    this.movementSpeed = direction.magnitude;
  }

  protected override void move(){
    if(this.frames < this.maxHomingFrames && this.frames > this.homingStartFrame){
      Vector3 acceleration = (Laser.player.transform.position - this.position).normalized * 0.14f * this.movementSpeed;
      this.direction += acceleration;
      this.direction = this.direction.normalized * this.movementSpeed;
    }
    this.frames++;

    if(this.hit()) return;

    this.trail.Add(this.position);
    this.position += this.direction;
    if(trail.Count > this.maxTrailCount){
      this.trail.RemoveAt(0);
    }
  }
}
