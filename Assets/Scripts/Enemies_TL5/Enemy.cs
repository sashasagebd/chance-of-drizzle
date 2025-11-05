using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy{
  private GameObject enemy;
  private EnemyController enemyController;
  public Enemy(GameObject enemy, Vector3 position){
    this.enemy = enemy;
    this.enemy.transform.position = position;
    this.enemyController = this.enemy.GetComponent<EnemyController>();
    this.enemyController.enemy = this;
  }
  public void Update(){
    this.enemy.transform.position = new Vector3(0, Random.Range(0, 10), 0);
  }
}
