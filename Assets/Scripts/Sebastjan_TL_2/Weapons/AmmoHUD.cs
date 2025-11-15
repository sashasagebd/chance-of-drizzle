using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AmmoHUD : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI ammoText;        // Text counter
    public Transform iconContainer;         // Parent for bullet icons
    public GameObject bulletPrefab;         // Bullet icon prefab

    [Header("Weapon Inventory")]
    public WeaponInventory inventory;       // Reference to your inventory

    [Header("Icon Settings")]
    public Vector2 iconSize = new Vector2(16, 16);  // Size of each bullet icon
    public int iconsPerRow = 10;                     // Max bullets per row
    public float spacingX = 2f;                      // Horizontal spacing
    public float spacingY = 2f;                      // Vertical spacing

    private WeaponBase _current;
    private List<Image> bulletIcons = new List<Image>();

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

                // Build icons according to magazine size
                BuildIcons(_current.magazineSize);

                // Update immediately
                OnAmmoChanged(_current.ammo, _current.magazineSize);
            }
            else
            {
                // No weapon selected
                ammoText.text = "--/--";
                ClearIcons();
                ammoText.text = "--/--";
            }
        }
    }

    void BuildIcons(int count)
    {
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
        foreach (Transform child in iconContainer)
            Destroy(child.gameObject);

        bulletIcons.Clear();
    }

    void OnAmmoChanged(int current, int max)
    {
        // Update text
        if (ammoText != null)
            ammoText.text = $"{current}/{max}";

        // Update icons
        for (int i = 0; i < bulletIcons.Count; i++)
        {
            bulletIcons[i].enabled = (i < current);
        }
        ammoText.text = $"{current}/{max}";
    }
}
