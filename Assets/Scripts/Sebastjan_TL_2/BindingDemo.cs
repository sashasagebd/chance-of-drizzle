using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Demonstrates static vs dynamic binding with VISUAL feedback
/// Attach this to a weapon GameObject and assign the muzzle flash particle system
/// </summary>
public class BindingDemo : MonoBehaviour
{
    [Header("Visual Feedback - Gun Muzzle Flash")]
    public ParticleSystem muzzleFlash; // Assign your weapon's muzzle flash here
    public ParticleSystem muzzleFlash2; // Assign your weapon's muzzle flash here


    [Header("Controls")]
    [Tooltip("Switch between Fire and Ice damage")]
    public bool useFire = true;

    private DamageEffect effect;
    private ParticleSystem.MainModule muzzleMain;
    private ParticleSystem.MainModule muzzleMain2;
    private PlayerInput playerInput;

    void Start()
    {
        if (muzzleFlash != null)
            muzzleMain = muzzleFlash.main;
        
        if (muzzleFlash2 != null)
            muzzleMain2 = muzzleFlash2.main;

        // Find PlayerInput component on parent (Player object)
        playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("BindingDemo requires a PlayerInput component on parent object!");
            return;
        }

        // Subscribe to the Color action
        var colorAction = playerInput.actions.FindAction("Color");
        if (colorAction != null)
        {
            colorAction.performed += OnColorPerformed;
            Debug.Log("Color action successfully subscribed!");
        }
        else
        {
            Debug.LogError("Color action not found in Input Actions!");
        }

        UpdateDemo();
    }

    void OnDestroy()
    {
        // Unsubscribe when destroyed
        if (playerInput != null)
        {
            var colorAction = playerInput.actions.FindAction("Color");
            if (colorAction != null)
            {
                colorAction.performed -= OnColorPerformed;
            }
        }
    }

    // Called when Color input is pressed
    private void OnColorPerformed(InputAction.CallbackContext context)
    {
        useFire = !useFire;
        UpdateDemo();

        // Fire the weapon to see the effect immediately
        if (muzzleFlash != null)
            muzzleFlash.Emit(1);
        
        if (muzzleFlash2 != null)
            muzzleFlash2.Emit(1);

        Debug.Log($"Color toggled! Now using: {(useFire ? "Fire" : "Ice")}");
    }

    void UpdateDemo()
    {
        // IMPORTANT: Static type is ALWAYS DamageEffect
        // Dynamic type changes based on useFire

        if (useFire)
        {
            effect = new FireDamageEffect();  // Static: DamageEffect, Dynamic: FireDamageEffect
        }
        else
        {
            effect = new IceDamageEffect();   // Static: DamageEffect, Dynamic: IceDamageEffect
        }

        // DYNAMICALLY BOUND METHOD (virtual)
        // Calls the method based on DYNAMIC type (runtime type)
        Color effectColor = effect.GetColor(); // Uses virtual method - returns Fire/Ice color


        // ===== VISUAL DEMONSTRATION =====
        // Change muzzle flash color based on DYNAMIC TYPE (via virtual GetColor method)
        if (muzzleFlash != null && muzzleFlash2 != null)
        {
            muzzleMain.startColor = effectColor;
            muzzleMain2.startColor = effectColor;
            Debug.Log($"🎨 Muzzle flash color changed to: {effectColor} (Dynamic binding!)");
        }
    }
}