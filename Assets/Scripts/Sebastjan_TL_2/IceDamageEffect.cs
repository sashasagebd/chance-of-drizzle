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

    // Override NON-VIRTUAL method - this will NOT be called when the static type is DamageEffect
    public new string GetBasicInfo()
    {
        return $"Ice class says: {effectName} - IT'S COLD!";
    }

    public new Color GetColor()
    {
        return Color.cyan;
    }
}