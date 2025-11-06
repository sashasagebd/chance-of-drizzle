using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class GetHexagonalPositionTest {
  private EnemyHub enemyHubScript;

  [UnitySetUp]
  public IEnumerator UnitySetup(){
    yield return new WaitForFixedUpdate();
    SceneManager.LoadSceneAsync("SampleScene");
    yield return new WaitForSeconds(1f);

    GameObject enemyHub = GameObject.Find("Enemy Hub");
    enemyHubScript = enemyHub.GetComponent<EnemyHub>();

    yield return null;
  }

  [UnityTest]
  public IEnumerator SpawnTestWithEnumeratorPasses(){
    if(enemyHubScript.runTests("TEST_GET_HEXAGON_POSITION") > 0){
      Assert.Fail("Hexagonal position mismatch");
    }
    yield return null;
  }
}
