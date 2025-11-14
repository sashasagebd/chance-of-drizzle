using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Missile : Laser{
  protected float frames = 0;
  protected int homingStartFrame = 6;
  protected int maxHomingFrames = 60;
  protected float movementSpeed = 1f;
  protected float homingStrength = 0.14f;

  public Missile(GameObject lineRenderer, Vector3 position, Vector3 direction, float damage, int homingStartFrame, int maxHomingFrames, float homingStrength)
  : base(lineRenderer, position, direction, damage){
  
    // Change line renderer parameters
    this.lineRenderer.startWidth = 0.08f;
    this.lineRenderer.endWidth = 0.05f;

    // Set up missile parameters
    this.nonStraightPath = true;
    this.movementSpeed = direction.magnitude;
    this.homingStartFrame = homingStartFrame;
    this.maxHomingFrames = maxHomingFrames;
    this.homingStrength = homingStrength;
  }

  protected override void move(){
    // If within homing time range
    if(this.frames < this.maxHomingFrames && this.frames > this.homingStartFrame){
      // Accelerate towards player
      Vector3 acceleration = (Laser.player.transform.position - this.position).normalized * this.homingStrength * this.movementSpeed;
      this.direction += acceleration;
      this.direction = this.direction.normalized * this.movementSpeed * Time.deltaTime * 60f;
    }
    this.frames += Time.deltaTime * 60f;

    // Stop laser at hit point for one last render
    if(this.hit()) return;

    // Move, add to trail, remove old trail
    this.trail.Add(this.position);
    this.position += this.direction;
    if(trail.Count > this.maxTrailCount){
      this.trail.RemoveAt(0);
    }
  }
}
