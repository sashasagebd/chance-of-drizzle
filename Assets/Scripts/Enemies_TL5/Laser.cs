using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Laser{
  protected static EnemyHub enemyHub;
  protected static GameObject player;

  protected GameObject gameObject;
  protected LineRenderer lineRenderer;
  protected Vector3 position;
  protected Vector3 direction;
  protected float time = 0f;
  protected List<Vector3> trail = new List<Vector3>();
  protected float distance = 0f;
  protected int maxTrailCount = 25;
  protected bool dead = false;
  protected bool nonStraightPath = false;
  protected float damage = 1f;

  public Laser(GameObject lineRenderer, Vector3 position, Vector3 direction, float damage = 1f){
    this.gameObject = lineRenderer;
    this.lineRenderer = lineRenderer.GetComponent<LineRenderer>();
    this.position = position;
    this.direction = direction;
    this.damage = damage;

    this.move();
    if(!this.dead) this.move();
  }
  protected void draw(){
    this.lineRenderer.positionCount = this.nonStraightPath ? 1 + this.trail.Count : 2;
    this.lineRenderer.SetPosition(0, this.position);
    if(this.nonStraightPath){
      for(int i = 0; i < this.trail.Count; i++){
        this.lineRenderer.SetPosition(this.trail.Count - i, this.trail[i]);
      }
    }else{
      this.lineRenderer.SetPosition(1, this.trail[0]);
    }
  }
  protected bool hit(){
    RaycastHit hit;
    if(Physics.Linecast(this.position, this.position + this.direction, out hit)){
      if (hit.transform.gameObject == Laser.player){
        Health health = hit.transform.gameObject.GetComponent<Health>();
        int damage = (int)(Mathf.Floor(this.damage)) + (Random.Range(0f, 1f) < this.damage % 1f ? 1 : 0);
        if(damage != 0) health.ApplyDamage(damage);
        // Debug.Log("hit player at " + hit.point);
        // this.player.GetComponent<PlayerController3D>().takeDamage(this.damage);
      }
      this.position = hit.point;
      this.dead = true;
      return true;
    }
    return false;
  }
  protected virtual void move(){
    if(this.hit()) return;

    this.trail.Add(this.position);
    this.position += this.direction * Time.deltaTime * 60f;
    if(trail.Count > this.maxTrailCount){
      this.trail.RemoveAt(0);
    }
  }
  public virtual bool run(){
    if(dead){
      return true;
    }

    move();
    draw();

    time += Time.deltaTime;
    if(time > 10f) dead = true;

    return false;
  }
  public GameObject getGameObject(){
    return this.gameObject;
  }
  public static void setStaticValues(GameObject player, EnemyHub enemyHub){
    Laser.player = player;
    Laser.enemyHub = enemyHub;
  }
}
