using UnityEngine;

public class SpawnEnemy : MonoBehaviour{
  void Start(){
    EnemyHub enemyHub = this.GetComponent<EnemyHub>();
    enemyHub.spawnEnemyAtTerrainHeight(new Vector2(2f, 0f), "basic");
  }
}
