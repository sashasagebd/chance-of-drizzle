

public class Armor : Item
{
    public float Defense { get; set; }
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

    public override void TriggerVisualEffects(object target)
    {
    if (target is PlayerController3D player)
        {
            if(VisualEffects.Instance != null)
            {
                if(ArmorType == "chestplate")
                {
                    VisualEffects.Instance.PlayVisual("PuffLarge", player.transform.position);
                }
                else
                {
                    VisualEffects.Instance.PlayVisual("Puff_HighPerformance", player.transform.position);
                }
            }
            
        }     
    }  
}