using UnityEngine;

public class EnemyController : MonoBehaviour{
  public Enemy enemy;
  void Awake(){}

  public void takeDamage(float damage){
    // Pass damage to dynamic class
    this.enemy.takeDamage(damage);
  }

  void Update(){
    // Update dynamic class
    enemy.Update();
  }
}
