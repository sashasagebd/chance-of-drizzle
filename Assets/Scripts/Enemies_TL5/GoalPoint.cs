using UnityEngine;

public class GoalPoint : MonoBehaviour{
  GameObject player;
  void Start(){
    player = GameObject.Find("Player");
  }
  void Update(){
    if(isInRange()){
      print("You win!");
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
}
