using UnityEngine;

/// <summary>
/// Subclass representing fire damage
/// </summary>
public class FireDamageEffect : DamageEffect
{
    public FireDamageEffect()
    {
        effectName = "FIRE DAMAGE";
        // effectColor = Color.red;
    }

    // Override NON-VIRTUAL method - this will NOT be called when the static type is DamageEffect
    public new string GetBasicInfo()
    {
        return $"Fire class says: {effectName} - IT'S HOT!";
    }

    public override Color GetColor()
    {
        return Color.green;
    }
}