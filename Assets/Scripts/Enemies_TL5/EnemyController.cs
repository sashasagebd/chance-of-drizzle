using UnityEngine;

public class EnemyController : MonoBehaviour{
  public Enemy enemy;
  void Awake(){}

  public void takeDamage(float damage){
    this.enemy.takeDamage(damage);
  }

  void Update(){
    enemy.Update();
  }
}
