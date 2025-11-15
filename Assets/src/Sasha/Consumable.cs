using UnityEngine;
using System;

public class Consumable : Item
{
    public int Duration { get; set; }
    public string EffectType { get; set; }
    public float Amount { get; set; }

    public Consumable(string name, string description, string effect, float amount, int duration)
    {
        Name = name;
        Description = description;
        EffectType = effect;
        Amount = amount;
        Duration = duration;
        CanStack = true;
    }


    public override void Use(object target)
    {
        if (target is PlayerController3D player)
        {
            if (EffectType == "Speed")
            {
                player.ApplySpeed(Amount, Duration);
            }
            else if (EffectType == "Heal")
            {
                if (player.HealthComponent != null)
                {
                    player.HealthComponent.Heal((int)Amount);
                }
            }
            else if (EffectType == "Jump")
            {
                player.ApplyJumpBoost(Amount, Duration);
            }
        }
    }
}