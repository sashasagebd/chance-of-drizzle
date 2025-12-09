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

    // PUBLIC: Override NON-VIRTUAL method - demonstrates static vs dynamic binding for educational purposes
    public new string GetBasicInfo()
    {
        return $"Fire class says: {effectName} - IT'S HOT!";
    }

    // PUBLIC: Override provides fire-specific color (educational demonstration of polymorphism)
    public override Color GetColor()
    {
        return Color.green;
    }
}