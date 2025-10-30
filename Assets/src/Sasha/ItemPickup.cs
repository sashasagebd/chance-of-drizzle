using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string itemID;
    private void OnTriggerEnter(Collider other)
    {
        PlayerController3D player = other.GetComponent<PlayerController3D>();

        if (player == null)
        {
            return;
        }
        
        Item itemToUse = ItemFactory.CreateItem(itemID);

        if (itemToUse == null)
        {
            return;
        }

        itemToUse.Use(player);

        if (itemToUse is Equipment)
        {
            bool added = Inventory.Instance.Add(itemToUse);
            if (added)
            {
                Debug.Log($"Added {itemToUse.Name} to inventory");
            }
            else
            {
                Debug.Log($"Unable to add {itemToUse.Name} to inventory");
            }
        }
        else if(itemToUse is Armor armor)
        {
            Armor replacedArmor;
            bool equipped = player.EquipArmor(armor, out replacedArmor);

            if (equipped)
            {
                // New armor was equipped — add it to inventory
                Inventory.Instance.Add(armor);

                if (replacedArmor != null)
                {
                    // Remove old armor from inventory if replaced
                    Inventory.Instance.Remove(replacedArmor);
                }
            }
            else
            {
                // Worse or equal — do not equip, do not add
                Debug.Log($"{armor.Name} was not equipped since it is worse or equal quality than current armor");
            }
        }
        else // Consumables not added to inventory
        {
            Debug.Log($"{itemToUse.Name} was consumed on pickup");
        }
        
        Destroy(gameObject);
    }
}