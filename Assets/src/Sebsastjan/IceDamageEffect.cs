using UnityEngine;

/// <summary>
/// Subclass representing ice damage
/// </summary>
public class IceDamageEffect : DamageEffect
{
    public IceDamageEffect()
    {
        effectName = "ICE DAMAGE";
        // effectColor = Color.cyan;
    }

    // PUBLIC: Override NON-VIRTUAL method - demonstrates static vs dynamic binding for educational purposes
    public new string GetBasicInfo()
    {
        return $"Ice class says: {effectName} - IT'S COLD!";
    }

    // PUBLIC: Provides ice-specific color (educational demonstration, should use override instead of new)
    public new Color GetColor()
    {
        return Color.cyan;
    }
}