using UnityEngine;

/// <summary>
/// Base class demonstrating static vs dynamic binding
/// </summary>
public class DamageEffect
{
    protected string effectName = "Generic Damage";
    protected Color effectColor = Color.white;

    // PUBLIC: Virtual method demonstrating dynamic binding - overridden by subclasses (FireDamageEffect, IceDamageEffect)
    public virtual string GetEffectDescription()
    {
        return $"{effectName} Effect - Color: {effectColor}";
    }

    // PUBLIC: Demonstrates a static binding pattern for educational purposes
    public string GetBasicInfo()
    {
        return $"Base class says: GENERIC DAMAGE";
    }

    // PUBLIC: Virtual method for subclasses to provide their own color implementation
    public virtual Color GetColor()
    {
        return effectColor;
    }
}