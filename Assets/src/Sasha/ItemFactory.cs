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
                    duration: 30
                );
            case "damagepotion":
                return new Consumable(
                    name: "Damage Potion",
                    description: "Temporarily increases damage.",
                    effect: "Damage",
                    amount: 1f,
                    duration: 30
                );
            case "helmet":
                return new Armor(
                    name: "Helmet",
                    description: "Protects the user's head",
                    armorType: "helmet",
                    defense: 2
                );
            case "chestplate":
                return new Armor(
                    name: "Chestplate",
                    description: "Protects the user's upper body",
                    armorType: "chestplate",
                    defense: 5
                );
            case "pants":
                return new Armor(
                    name: "Pants",
                    description: "Protects the user's legs",
                    armorType: "pants",
                    defense: 4
                );
            
            default:
                Debug.LogError($"{id} is not recognized as a valid item");
                return null;
        }
    }

}