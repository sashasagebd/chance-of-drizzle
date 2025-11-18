using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class SpawnTest{
  private EnemyHub enemyHubScript;

  [UnitySetUp]
  public IEnumerator UnitySetup(){
    yield return new WaitForFixedUpdate();
    SceneManager.LoadSceneAsync("Level_0");
    yield return new WaitForSeconds(1f);

    GameObject enemyHub = GameObject.Find("Enemy Hub");
    enemyHubScript = enemyHub.GetComponent<EnemyHub>();
    //enemyHubScript.enemy = enemy;

    /*
    var camera = new GameObject("Camera").AddComponent<Camera>();
    camera.orthographic = true;
    camera.transform.position = new Vector3(0, 10, 0);
    //*/

    yield return null;
  }

  [UnityTest]
  public IEnumerator SpawnTestWithEnumeratorPasses(){
    yield return new WaitForSeconds(1f);
    int iterations = 0;
    int maxIterations = 5000;
    while(iterations < maxIterations){
      iterations++;
      if(enemyHubScript.runTests("SPAWN_ENEMIES_AT_TERRAIN_HEIGHT") > 0){
        Assert.Fail("Spawn Enemies failed at " + iterations);
      }
      if(iterations % 50 == 0){
        yield return null;
      }
    }
  }
}
