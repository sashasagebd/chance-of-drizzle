using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class CanReadTerrainTest {
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
    if(enemyHubScript.runTests("CHECK_TERRAIN_EXISTS") > 0){
      Assert.Fail("Failed to read the terrain. The terrain may not exist, may not be in the terrain layer, or the function is broken.");
    }
    yield return null;
  }
}
