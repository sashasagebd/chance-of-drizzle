

public class Equipment : Item
{
    
    public string EffectType { get; set; }
    public float Amount { get; set; }

    public Equipment(string name, string description, string effectType, float amount)
    {
        Name = name;
        Description = description;
        EffectType = effectType;
        Amount = amount;
        CanStack = true;
    }

    public override void Use(object target)
    {
        if (target is PlayerController3D player)
        {
            //need to make an equip method in player controller
        }
    }
}