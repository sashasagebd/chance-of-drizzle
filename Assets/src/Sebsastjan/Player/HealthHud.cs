using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Image fillBar;
    [SerializeField] private Image chipBar;
    [SerializeField] private CanvasGroup damageFlash;

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 6f;
    [SerializeField] private float chipDelay = 0.3f;
    [SerializeField] private float chipSpeed = 2f;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float lowHealthThreshold = 0.25f;
    [SerializeField] private float lowHealthPulseSpeed = 5f;

    private float targetFill = 1f;

    void Start()
    {
        if (health == null)
            Debug.LogWarning("HealthHUD: no Health assigned.");

        targetFill = 1f;
        if (fillBar) fillBar.fillAmount = 1f;
        if (chipBar) chipBar.fillAmount = 1f;
        if (damageFlash) damageFlash.alpha = 0f;
    }

    void Update()
    {
       
        if (fillBar)
            fillBar.fillAmount = Mathf.Lerp(fillBar.fillAmount, targetFill, Time.deltaTime * smoothSpeed);

     
        if (fillBar && targetFill <= lowHealthThreshold)
        {
            float pulse = (Mathf.Sin(Time.time * lowHealthPulseSpeed) + 1f) / 2f;
            fillBar.color = Color.Lerp(new Color(0.6f,0.1f,0.1f), Color.red, pulse);
        }
    }

    // PUBLIC: Called by Health class when health changes to update UI display
    public void ApplyHealthChange(float oldHp, float newHp)
    {
        float oldPercent = Mathf.Clamp01(oldHp / health.maxHp);
        float newPercent = Mathf.Clamp01(newHp / health.maxHp);

        // If taking damage
        if (newHp < oldHp)
        {
            targetFill = newPercent;
            if (damageFlash) StartCoroutine(DamageFlash());
            StartCoroutine(AnimateChipBar());
        }
        else // healing
        {
            targetFill = newPercent;
            if (chipBar) chipBar.fillAmount = newPercent; // make chip bar follow instantly on heal
        }
    }

    IEnumerator AnimateChipBar()
    {
        if (chipBar == null) yield break;
        yield return new WaitForSeconds(chipDelay);

        // Smoothly move chipBar down to targetFill
        while (chipBar.fillAmount > targetFill + 0.001f)
        {
            chipBar.fillAmount = Mathf.Lerp(chipBar.fillAmount, targetFill, Time.deltaTime * chipSpeed);
            yield return null;
        }
        chipBar.fillAmount = targetFill;
    }

    IEnumerator DamageFlash()
    {
        if (damageFlash == null) yield break;

        float t = 0f;
        float start = 0.6f; // starting alpha
        damageFlash.alpha = start;

        while (t < flashDuration)
        {
            damageFlash.alpha = Mathf.Lerp(start, 0f, t / flashDuration);
            t += Time.deltaTime;
            yield return null;
        }
        damageFlash.alpha = 0f;
    }
}
