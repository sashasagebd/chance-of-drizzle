using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class SpawnTestBoundary {
  private EnemyHub enemyHubScript;

  [UnitySetUp]
  public IEnumerator UnitySetup(){
    yield return new WaitForFixedUpdate();
    SceneManager.LoadSceneAsync("Level_0");
    yield return new WaitForSeconds(1f);

    GameObject enemyHub = GameObject.Find("Enemy Hub");
    enemyHubScript = enemyHub.GetComponent<EnemyHub>();

    yield return null;
  }

  [UnityTest]
  public IEnumerator SpawnTestWithEnumeratorPasses(){
    if(enemyHubScript.runTests("SPAWN_ENEMIES_AT_TERRAIN_HEIGHT") > 0){
      Assert.Fail("Failed to spawn enemy");
    }
    yield return null;
  }
}
