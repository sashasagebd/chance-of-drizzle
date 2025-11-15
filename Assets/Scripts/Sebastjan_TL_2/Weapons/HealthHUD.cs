using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUD : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI healthText;  // Drag your HealthText here
    public Slider healthSlider;         // Drag your Health Slider here
    public Image sliderFillImage;       // Drag the Fill image from the slider here

    [Header("Player Reference")]
    public Health playerHealth;         // Drag your player's Health component here

    [Header("Colors")]
    public Color highHealthColor = Color.green;     // > 50% health
    public Color mediumHealthColor = Color.yellow;  // 25-50% health
    public Color lowHealthColor = Color.red;        // < 25% health

    void Start()
    {
        if (!playerHealth)
        {
            Debug.LogError("HealthHUD: playerHealth not assigned!");
            return;
        }

        // Subscribe to health change events
        playerHealth.OnHealthChanged += UpdateHealthDisplay;

        // Initialize display with current health
        UpdateHealthDisplay(playerHealth.Current, playerHealth.maxHp);
    }

    void OnDestroy()
    {
        // Unsubscribe when destroyed to prevent memory leaks
        if (playerHealth)
        {
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;
        }
    }

    void UpdateHealthDisplay(float current, float max)
    {
        // Update text
        if (healthText)
        {
            healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        // Update slider
        if (healthSlider)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        // Update color based on health percentage
        if (sliderFillImage)
        {
            float healthPercent = current / max;

            if (healthPercent > 0.5f)
            {
                sliderFillImage.color = highHealthColor;
            }
            else if (healthPercent > 0.25f)
            {
                sliderFillImage.color = mediumHealthColor;
            }
            else
            {
                sliderFillImage.color = lowHealthColor;
            }
        }
    }
}
