

public class Armor : Item
{
    public int Defense { get; set; }
    public string ArmorType { get; set; }

    public Armor(string name, string description, string armorType, int defense)
    {
        Name = name;
        Description = description;
        ArmorType = armorType;
        Defense = defense;
        CanStack = false;
    }

    public override void Use(object target)
    {
        if (target is PlayerController3D player)
        {
            //need to make a armor method for player
        }
    }
}