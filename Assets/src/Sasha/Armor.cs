

public class Armor : Item
{
    public int Defense { get; set; }
    public string ArmorType { get; set; }

    public Armor(string name, string description, string armorType, float defense)
    {
        Name = name;
        Description = description;
        ArmorType = armorType;
        Defense = defense;
        CanStack = false;
    }

    public override void Use(object target)
    {
        
    }
}