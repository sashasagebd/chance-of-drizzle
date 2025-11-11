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
            case "speedboost":
                return new Consumable(
                    name: "Speed Boost",
                    description: "Temporarily increases movement speed.",
                    effect: "Speed",
                    amount: 2f,
                    duration: 30
                );
            case "jumpboost":
                return new Consumable(
                    name: "Jump Boost",
                    description: "Temporarily increases jump height",
                    effect: "Jump",
                    amount: 2f,
                    duration: 20
                );
            /*case "damagepotion":
                return new Consumable(
                    name: "Damage Potion",
                    description: "Temporarily increases damage.",
                    effect: "Damage",
                    amount: 1f,
                    duration: 30
                );*/
            case "healthupgrade":
                return new Equipment(
                    name: "Health Upgrade",
                    description: "Increases max health by 5",
                    effect: "HealthIncrease",
                    amount: 5
                );
            case "speedupgrade":
                return new Equipment(
                    name: "Speed Upgrade",
                    description: "Increases speed by 1",
                    effect: "Speed",
                    amount: 1f
                );
            case "damageupgrade":
                return new Equipment(
                    name: "Damage Upgrade",
                    description: "Increases damage by 1",
                    effect: "Damage",
                    amount: 1
                );
            case "ironhelmet":
                return new Armor(
                    name: "Helmet",
                    description: "Protects the user's head",
                    armorType: "helmet",
                    defense: .05 //5%
                );
            case "ironchestplate":
                return new Armor(
                    name: "Chestplate",
                    description: "Protects the user's upper body",
                    armorType: "chestplate",
                    defense: .1 //10%
                );
            case "ironpants":
                return new Armor(
                    name: "Pants",
                    description: "Protects the user's legs",
                    armorType: "pants",
                    defense: .05 //5%
                );
            case "leatherhelmet":
                return new Armor(
                    name: "Helmet",
                    description: "Protects the user's head",
                    armorType: "helmet",
                    defense: .025 //2.5%
                );
            case "leatherchestplate":
                return new Armor(
                    name: "Chestplate",
                    description: "Protects the user's upper body",
                    armorType: "chestplate",
                    defense: .05 //5%
                );
            case "leatherpants":
                return new Armor(
                    name: "Pants",
                    description: "Protects the user's legs",
                    armorType: "pants",
                    defense: .025 //2.5%
                );
            case "ancienthelmet":
                return new Armor(
                    name: "Helmet",
                    description: "Protects the user's head",
                    armorType: "helmet",
                    defense: .075 //7.5%
                );
            case "ancientchestplate":
                return new Armor(
                    name: "Chestplate",
                    description: "Protects the user's upper body",
                    armorType: "chestplate",
                    defense: .15 //15%
                );
            case "ancientpants":
                return new Armor(
                    name: "Pants",
                    description: "Protects the user's legs",
                    armorType: "pants",
                    defense: .075 //7.5%
                );
            default:
                Debug.LogError($"{id} is not recognized as a valid item");
                return null;
        }
    }

}