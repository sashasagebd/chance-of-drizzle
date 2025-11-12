using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : ObjectSpawner
{

    public override bool SpecificTest(GameObject testObject) {

        Debug.Log(testObject.GetComponent<ItemPickup>());
        if (testObject.GetComponent<ItemPickup>() != null) {
            // Enemy.createEnemy
            // Debug.Log("True!");
            return true;
        } else {
            // Debug.Log("False!");
            return false;
        }
    }

    /*
    public virtual void Initialize() {
        
    }
    */
}
