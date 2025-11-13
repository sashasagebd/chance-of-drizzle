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
    // Eventually will have a Chest object which will drop these items, but for the time being they will just spawn in place via normal means

    /*
    public virtual void Initialize() {
        
    }
    */
}
