using UnityEngine;
using System.Collections;

public static class ItemFactory
{
    public static Item CreateItem(string id)
    {
        string lowerId = id.ToLowerInvariant();
        switch (id)
        {
            case "healpotion":
                return new Consumable(
                    name: "Healing Potion",
                    description: "Restores a small amount of health.",
                    effect: "Heal",
                    amount: 10f,
                    duration: 0
                );
            case "speedberry":
                return new Consumable(
                    name: "Speed Berry",
                    description: "Temporarily increases movement speed.",
                    effect: "Speed",
                    amount: 2f,
                    duration: 5
                );
            default:
                Debug.LogError($"{id} is not recognized as a valid item");
                return null;
        }
    }

}