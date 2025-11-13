using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class EnemyRelatedTests {
  private EnemyHub enemyHubScript;

  /*
"flying"
"flying-pyramid"
"flying-double"
"flying-melee-quad"
"flying-sniper"
"flying-melee"
"flying-ufo"
"flying-missile"

"basic"
"melee"
"melee-figure-eight"
"quad"
"melee-figure-eight-double"
"homing-shot"
"melee-egg-beater"
"drizzle-of-doom"
  */

  [UnitySetUp]
  public IEnumerator UnitySetup(){
    yield return new WaitForFixedUpdate();
    SceneManager.LoadSceneAsync("SampleScene");
    yield return new WaitForSeconds(1f);

    GameObject enemyHub = GameObject.Find("Enemy Hub");
    enemyHubScript = enemyHub.GetComponent<EnemyHub>();

    yield return null;
  }

  private static IEnumerable enemyTypes{
    get{
      yield return new TestCaseData("basic").Returns(null);
      yield return new TestCaseData("melee").Returns(null);
      yield return new TestCaseData("melee-figure-eight").Returns(null);
      yield return new TestCaseData("quad").Returns(null);
      yield return new TestCaseData("melee-figure-eight-double").Returns(null);
      yield return new TestCaseData("homing-shot").Returns(null);
      yield return new TestCaseData("melee-egg-beater").Returns(null);
      yield return new TestCaseData("drizzle-of-doom").Returns(null);
      yield return new TestCaseData("flying").Returns(null);
      yield return new TestCaseData("flying-pyramid").Returns(null);
      yield return new TestCaseData("flying-double").Returns(null);
      yield return new TestCaseData("flying-melee-quad").Returns(null);
      yield return new TestCaseData("flying-sniper").Returns(null);
      yield return new TestCaseData("flying-melee").Returns(null);
      yield return new TestCaseData("flying-ufo").Returns(null);
      yield return new TestCaseData("flying-missile").Returns(null);
    }
  }
  // https://discussions.unity.com/t/testcasesource-compatible-with-unitytest/817168/4
  [UnityTest] [TestCaseSource(nameof(enemyTypes))]
  public IEnumerator checkEnemyParameters(string enemyType){
    yield return null;
    Enemy enemy = Enemy.createEnemy(new Vector3(0f, 0f, 0f), enemyType);
    int exitCode = enemy.runTests("PARAMETERS_ARE_DEFINED");
    if(exitCode != 0){
      if(exitCode == 1){
        Assert.Fail("Max health is not defined");
      }else if(exitCode == 2){
        Assert.Fail("Damage is not defined");
      }else if(exitCode == 3){
        Assert.Fail("Reload time is not defined or sword range is not defined (for melee)");
      }else if(exitCode == 4){
        Assert.Fail("Movement speed is not defined");
      }
    }
    yield return null;
  }

  [UnityTest] [TestCaseSource(nameof(enemyTypes))]
  public IEnumerator checkEnemyCanRun(string enemyType){
    yield return null;
    Enemy enemy = Enemy.createEnemy(new Vector3(0f, 0f, 0f), enemyType);
    enemy.Update();
    int exitCode = enemy.runTests("GAME_OBJECT_EXISTS");
    if(exitCode != 0){
      if(exitCode == 1){
        Assert.Fail("Max health is not defined");
      }else if(exitCode == 2){
        Assert.Fail("Damage is not defined");
      }else if(exitCode == 3){
        Assert.Fail("Reload time is not defined or sword range is not defined (for melee)");
      }else if(exitCode == 4){
        Assert.Fail("Movement speed is not defined");
      }
    }
    yield return null;
  }
}
