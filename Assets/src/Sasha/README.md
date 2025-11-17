My feature is the items and inventory in our game. 

-The Item class is abstract and some functions are overridden by each of its subclasses (Equipment, Armor, and Consumable). 

-These subclasses need to override the use function since each type of item has different things needed when being used. 

-The Inventory and InventorySlots classes track all currently equipped items with these specifications: only 1 of each type of armor (e.g. chestplate) can be equipped at a time and consumables don't enter the inventory since they only last a set duration. 

-The ItemPickup class allows items to be picked up by the player.

-The ItemFactory class is what handles all item creation, with a key determining what class of item is being created and what stat bonuses it contains.

-The VisualEffects class handles visual effects on item pickup, with unique effects being played depending on the type of item.

