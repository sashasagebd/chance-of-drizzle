using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Call of Duty-style ammo display with large magazine count and small reserve count.
/// Displays weapon name above ammo in bottom-right corner.
/// Format: "ASSAULT RIFLE"
///         "30 | 120" (magazine | reserve)
/// </summary>
public class AmmoHUD : MonoBehaviour
{
    [Header("UI Elements - CoD Style")]
    [SerializeField] private TextMeshProUGUI weaponNameText;      // Weapon name display
    [SerializeField] private TextMeshProUGUI magazineText;        // Large magazine count (e.g., "30")
    [SerializeField] private TextMeshProUGUI separatorText;       // Separator ("|")
    [SerializeField] private TextMeshProUGUI reserveText;         // Small reserve count (e.g., "120")

    [Header("Weapon Icon")]
    [SerializeField] private Transform weaponIconContainer;       // Container for weapon 3D icon/prefab
    [SerializeField] private Vector3 iconScale = Vector3.one;     // Scale of weapon icon
    [SerializeField] private Vector3 iconRotation = Vector3.zero; // Rotation offset for weapon icon
    [SerializeField] private bool autoRotateIcon = false;         // Slowly rotate weapon icon
    [SerializeField] private float rotationSpeed = 30f;           // Rotation speed (degrees/sec)

    [Header("Weapon Inventory")]
    [SerializeField] private WeaponInventory inventory;           // Reference to weapon inventory

    [Header("Legacy Icon Support (Optional)")]
    [SerializeField] private bool useBulletIcons = false;         // Enable old bullet icon system
    [SerializeField] private Transform iconContainer;             // Parent for bullet icons
    [SerializeField] private GameObject bulletPrefab;             // Bullet icon prefab
    [SerializeField] private Vector2 iconSize = new Vector2(16, 16);
    [SerializeField] private int iconsPerRow = 10;
    [SerializeField] private float spacingX = 2f;
    [SerializeField] private float spacingY = 2f;

    [Header("Text Styling")]
    [SerializeField] private Color lowAmmoColor = Color.red;      // Color when magazine is low (<25%)
    [SerializeField] private Color normalAmmoColor = Color.white; // Normal ammo color

    private WeaponBase _current;
    private List<Image> bulletIcons = new List<Image>();
    private GameObject _currentWeaponIcon; // Currently displayed weapon icon instance

    void Update()
    {
        if (!inventory) return;

        // Detect weapon switch
        if (_current != inventory.Current)
        {
            // Unsubscribe previous weapon
            if (_current != null)
                _current.OnAmmoChanged -= OnAmmoChanged;

            _current = inventory.Current;

            if (_current != null)
            {
                _current.OnAmmoChanged += OnAmmoChanged;

                // Update weapon name
                if (weaponNameText != null)
                    weaponNameText.text = _current.weaponName.ToUpper();

                // Update weapon icon
                UpdateWeaponIcon(_current);

                // Build icons if legacy system is enabled
                if (useBulletIcons)
                    BuildIcons(_current.magazineSize);

                // Update immediately
                OnAmmoChanged(_current.ammo, _current.magazineSize, _current.reserveAmmo);
            }
            else
            {
                // No weapon selected
                if (weaponNameText != null) weaponNameText.text = "";
                if (magazineText != null) magazineText.text = "--";
                if (separatorText != null) separatorText.text = "|";
                if (reserveText != null) reserveText.text = "--";

                ClearWeaponIcon();

                if (useBulletIcons)
                    ClearIcons();
            }
        }

        // Auto-rotate weapon icon if enabled (rotates on gun's long axis)
        if (autoRotateIcon && _currentWeaponIcon != null)
        {
            _currentWeaponIcon.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    /// <summary>
    /// Called when weapon ammo changes. Updates CoD-style display.
    /// </summary>
    void OnAmmoChanged(int currentAmmo, int maxMagazine, int reserve)
    {
        // Update magazine count (large text)
        if (magazineText != null)
        {
            magazineText.text = currentAmmo.ToString();

            // Change color if low on ammo
            float ammoPercent = (float)currentAmmo / maxMagazine;
            magazineText.color = ammoPercent <= 0.25f ? lowAmmoColor : normalAmmoColor;
        }

        // Update separator
        if (separatorText != null)
            separatorText.text = "|";

        // Update reserve count (smaller text)
        if (reserveText != null)
            reserveText.text = reserve.ToString();

        // Update legacy bullet icons if enabled
        if (useBulletIcons)
        {
            for (int i = 0; i < bulletIcons.Count; i++)
            {
                bulletIcons[i].enabled = (i < currentAmmo);
            }
        }
    }

    #region Legacy Bullet Icon System

    void BuildIcons(int count)
    {
        if (!useBulletIcons) return;

        ClearIcons();

        // Add GridLayoutGroup if not present
        GridLayoutGroup grid = iconContainer.GetComponent<GridLayoutGroup>();
        if (grid == null)
            grid = iconContainer.gameObject.AddComponent<GridLayoutGroup>();

        grid.cellSize = iconSize;
        grid.spacing = new Vector2(spacingX, spacingY);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = iconsPerRow;
        grid.childAlignment = TextAnchor.UpperRight;

        // Generate icons
        for (int i = 0; i < count; i++)
        {
            GameObject icon = Instantiate(bulletPrefab, iconContainer);
            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.sizeDelta = iconSize;
            bulletIcons.Add(icon.GetComponent<Image>());
        }
    }

    void ClearIcons()
    {
        if (!useBulletIcons) return;

        foreach (Transform child in iconContainer)
            Destroy(child.gameObject);

        bulletIcons.Clear();
    }

    #endregion

    #region Weapon Icon System

    /// <summary>
    /// Updates the displayed weapon icon when weapon changes.
    /// Instantiates the weapon's iconPrefab in the weaponIconContainer.
    /// </summary>
    void UpdateWeaponIcon(WeaponBase weapon)
    {
        // Clear previous icon
        ClearWeaponIcon();

        // Check if we have a container and the weapon has an icon prefab
        if (weaponIconContainer == null || weapon.weaponIconPrefab == null)
            return;

        // Instantiate the weapon icon
        _currentWeaponIcon = Instantiate(weapon.weaponIconPrefab, weaponIconContainer);

        // Reset position to container's center
        _currentWeaponIcon.transform.localPosition = Vector3.zero;

        // Apply custom scale
        _currentWeaponIcon.transform.localScale = iconScale;

        // Apply custom rotation offset
        _currentWeaponIcon.transform.localEulerAngles = iconRotation;

        // Disable any scripts on the icon (we just want it as a visual)
        // This prevents weapon logic from running on the HUD icon
        var scripts = _currentWeaponIcon.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            script.enabled = false;
        }

        // Optionally set layer to UI (layer 5)
        SetLayerRecursively(_currentWeaponIcon, 5);
    }

    /// <summary>
    /// Clears the currently displayed weapon icon.
    /// </summary>
    void ClearWeaponIcon()
    {
        if (_currentWeaponIcon != null)
        {
            Destroy(_currentWeaponIcon);
            _currentWeaponIcon = null;
        }
    }

    /// <summary>
    /// Recursively sets the layer of a GameObject and all its children.
    /// Useful for ensuring weapon icons render on the UI layer.
    /// </summary>
    void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;

        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    #endregion
}
