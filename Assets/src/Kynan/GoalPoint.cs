using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalPoint : MonoBehaviour{
  GameObject player;
  static string levelExitID = "WinGame";

  void Start(){
    player = GameObject.Find("Player");
    if(player == null){
      player = GameObject.Find("Player ");
    }
  }

  void Update(){
    if(isInRange()){
      SceneManager.LoadScene(levelExitID);
    }
  }

  private bool isInRange(){
    float distance = Vector3.Distance(transform.position, player.transform.position);
    if(distance > 3f){
      return false;
    }
    distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(player.transform.position.x, player.transform.position.z));
    if(distance > 2f){
      return false;
    }
    Vector2 vectorToPlayer = new Vector2(player.transform.position.x, player.transform.position.z) - new Vector2(transform.position.x, transform.position.z);
    float angleDifference = Mathf.PI * Vector2.Angle(new Vector2(transform.forward.x, transform.forward.z), vectorToPlayer) / 180f;
    float perpendicularDistance = Mathf.Abs(distance * Mathf.Cos(angleDifference));

    return perpendicularDistance < 0.25f;
  }

  /*
  private static bool canLevelBeLoaded(string exitName) {
    int sceneCount = SceneManager.sceneCountBuildInSettings;
    for (int i=0; i<sceneCount; i++) {
      string path = SceneUtility.GeBuildIndexByScenePath(scenePath);
      string name = Path.GetFileNameWithoutExtension(path);

      if (name == sceneId) return true;
    }
    return false;
  }
  */
  private static bool canLevelBeLoaded(string exitName) {

    for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
    {
        string levelPath = SceneUtility.GetScenePathByBuildIndex(i);
        string levelName = System.IO.Path.GetFileNameWithoutExtension(levelPath);

        if (levelName == exitName)
          return true;
    }
    return false;
  }

  // Following function written by Erik
  public static void setExitDestination(string exitName) {
    if (canLevelBeLoaded(exitName)) levelExitID = exitName;
    else { 
      Debug.LogWarning("Level exit has been set to invalid value " + exitName + " Defaulted to WinGame");
      levelExitID="WinGame";
    }
  }
}
