using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string itemID;
    private void OnTriggerEnter(Collider other)
    {
        PlayerController3D player = other.GetComponent<PlayerController3D>();

        if (player != null)
        {
            Item itemToUse = ItemFactory.CreateItem(itemID);

            if (itemToUse != null)
            {
                itemToUse.Use(player);
                UnityEngine.Debug.Log($"Player picked up and used: {itemToUse.Name} (ID: {itemID})");
                Destroy(gameObject);
            }
            else
            {
                UnityEngine.Debug.LogError($"[ItemPickup] Factory failed to create item with ID: {itemID}. Check the ID!");
            }
        }
    }
}