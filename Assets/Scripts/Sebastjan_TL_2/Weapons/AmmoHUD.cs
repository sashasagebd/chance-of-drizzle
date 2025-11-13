using TMPro;
using UnityEngine;

public class AmmoHUD : MonoBehaviour {
    public TextMeshProUGUI ammoText;  // drag your AmmoText here
    public WeaponInventory inventory;         // drag your Gun (ProjectileWeapon/HitscanWeapon)

    private WeaponBase _current;

    void Update()
    {
        if (!inventory)return;
        if (inventory.Current != _current)
        {
            // stop listening to prev weapon
            if (_current) _current.OnAmmoChanged -= OnAmmo;
            _current = inventory.Current;
            
            // start listening on the next weapon
            if (_current)
            {
                _current.OnAmmoChanged += OnAmmo;
                OnAmmo(_current.ammo, _current.magazineSize);
            }
            else
            {
                ammoText.text = "Ammo \n--/--";
            }
        }
    }

    void OnAmmo(int current, int max)
    {
        ammoText.text = $"Ammo \n{current}/{max}";
    }
}