using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIPlayer{
  private GameObject player;
  private PlayerController3D playerController;
  private EnemyHub enemyHub;
  private CharacterController characterController;
  private Camera camera;

  private Vector3 velocity = Vector3.zero;
  private Quaternion rotation = Quaternion.identity;

  public AIPlayer(GameObject player, EnemyHub enemyHub){
    this.player = player;
    this.playerController = player.GetComponent<PlayerController3D>();
    this.enemyHub = enemyHub;
    this.characterController = player.GetComponent<CharacterController>();
    this.camera = player.GetComponentInChildren<Camera>();
  }
  public bool run(){
    Vector3 closestEnemy = this.enemyHub.getClosestEnemy(this.player.transform.position, 1);
    Vector3 acceleration = closestEnemy - this.player.transform.position;

    this.velocity *= 0.75f;
    this.velocity += new Vector3(acceleration.x, 0f, acceleration.z).normalized * Mathf.Min(0.005f, acceleration.magnitude) * (-1f);
    this.characterController.Move(this.velocity);

    Quaternion lookRotation = Quaternion.LookRotation(closestEnemy - this.player.transform.position - new Vector3(0f, 0.8f, 0f));
    /*
    Vector3 eulerNumbers = lookRotation.eulerAngles;

    this.player.transform.rotation = Quaternion.Euler(0f, eulerNumbers.y, 0f);
    this.camera.transform.rotation = Quaternion.Euler(eulerNumbers.x, 0f, 0f);
    /*/
    this.rotation = Quaternion.RotateTowards(this.rotation, lookRotation, 1.6f);
    this.camera.transform.rotation = this.rotation;
    //*/

    var weapon = this.playerController.inventory ? this.playerController.inventory.Current : null;
    if(weapon != null){
      Vector3 origin = this.playerController.muzzle ? this.playerController.muzzle.position : this.camera.transform.position;
      Vector3 forward = this.camera.transform.forward;
      weapon.TryFire(origin, forward);
      weapon.Reload();
    }

    //Debug.Log(lookRotation + " " + eulerNumbers);
    return Input.anyKeyDown;
  }
}
