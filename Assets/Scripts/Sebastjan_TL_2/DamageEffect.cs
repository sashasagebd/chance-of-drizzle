using UnityEngine;

/// <summary>
/// Base class demonstrating static vs dynamic binding
/// </summary>
public class DamageEffect
{
    protected string effectName = "Generic Damage";
    protected Color effectColor = Color.white;

    // VIRTUAL method - uses dynamic binding (runtime type determines which method is called)
    public virtual string GetEffectDescription()
    {
        return $"{effectName} Effect - Color: {effectColor}";
    }

    public string GetBasicInfo()
    {
        return $"Base class says: GENERIC DAMAGE";
    }

    public virtual Color GetColor()
    {
        return effectColor;
    }
}