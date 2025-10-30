using UnityEngine;
using System;

public class Equipment : Item
{
    
    public string EffectType { get; set; }
    public float Amount { get; set; }

    public Equipment(string name, string description, string effect, float amount)
    {
        Name = name;
        Description = description;
        EffectType = effect;
        Amount = amount;
        CanStack = true;
    }

    public override void Use(object target)
    {
        if (target is PlayerController3D player)
        {
            
            if (EffectType == "Speed")
            {
                player.ApplySpeed(Amount, 0);
            }
            else if(EffectType == "HealthIncrease")
            {
                if(player.HealthComponent != null)
                {
                    player.HealthComponent.IncreaseMaxHealth((int)Amount);
                }
            }
        }
    }
}