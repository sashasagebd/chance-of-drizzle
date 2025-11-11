Information about all prefabs in this folder:

General:
    All of my item prefabs are created essentially the same way:
        -Item model for art on a gameobject
        -Item pickup script attached with specific item ID (for ItemFactory.CreateItem(ID)) entered in unity inspector
        -Box collider with is trigger set so item pickup will work

Armor: (armor cannot stack in inventory, best of each armor slot auto equipped)
    Leather: Full set gives 10% defense
        Helmet: 2.5% defense
        Chestplate: 5% defense
        Pants: 2.5% defense
    Iron: Full set gives 20% defense
        Helmet: 5% defense
        Chestplate: 10% defense
        Pants: 5% defense
    Ancient: Full set gives 30% defense
        Helmet: 7.5% defense
        Chestplate: 15% defense
        Pants: 7.5% defense

Equipment: (equipment can stack in inventory)
    Speed Upgrade: Permanently increases speed by 1f
    Health Upgrade: Permanently increases health by 5f
    Damage Upgrade: Permanently increases damage by 1

Consumables:
    Speed Boost: Increases speed by 2f for 30 seconds
    Heal Potion: Heals 10f health
    Jump Boost: Increases jump height by 2f for 20 seconds