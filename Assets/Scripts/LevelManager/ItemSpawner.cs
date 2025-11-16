using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : ObjectSpawner
{

    public override bool SpecificTest(GameObject testObject) {

        Debug.Log(testObject.GetComponent<ItemPickup>());
        if (testObject.GetComponent<ItemPickup>() != null) {
            // Debug.Log("True!");
            return true;
        } else {
            // Debug.Log("False!");
            
            return false;
        }
    }
    
    public override void Initialize() {
        if (beenInitialized && spawnChoice != null) {

            // Debug.Log("Spawning object!");
            
            GameObject item;

            if (spawnAtTerrainHeight && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, LayerMask.GetMask("Terrain")))
            {
                item = Instantiate(spawnChoice, hit.point + constDisplace, Quaternion.identity);
            } else {
                item = Instantiate(spawnChoice, transform.position, Quaternion.identity);
            }

            if (item!=null) {
                ItemPickup pickup = spawnChoice.GetComponent<ItemPickup>();
                pickup.hudManager = GameObject.Find("Hud/HudManager").GetComponent<HUDManager>();
            }
                
            Destroy(gameObject);
        }
    }
}
