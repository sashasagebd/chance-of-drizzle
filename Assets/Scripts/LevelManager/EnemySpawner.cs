using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : ObjectSpawner
{

    public override bool SpecificTest(GameObject testObject) {

        Debug.Log(testObject.GetComponent<EnemyController>());
        if (testObject.GetComponent<EnemyController>() != null) {
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
