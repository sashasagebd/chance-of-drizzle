using UnityEngine;

public class EnemyController : MonoBehaviour{
  public Enemy enemy;
  void Awake(){}
  void Update(){
    enemy.Update();
  }
}
