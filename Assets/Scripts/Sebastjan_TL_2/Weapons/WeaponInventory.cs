using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    [Tooltip("Assign in order (0,1,2...). Items can be children of FP_WeaponRig).")]
    public List<WeaponBase> weapons = new List<WeaponBase>();

    private int CurrentIndex { get; set; } = 0;
    public WeaponBase Current => (weapons.Count > 0 && CurrentIndex >= 0) ? weapons[CurrentIndex] : null;

    void Start() { Select(CurrentIndex); }

    private void Select(int index)
    {
        if (weapons.Count == 0) return;
        index = Mathf.Clamp(index, 0, weapons.Count - 1);

        for (int i = 0; i < weapons.Count; i++)
            if (weapons[i]) weapons[i].gameObject.SetActive(i == index);

        CurrentIndex = index;
    }

    public void Next() { if (weapons.Count > 0) Select( (CurrentIndex + 1) % weapons.Count ); }
    public void Prev() { if (weapons.Count > 0) Select( (CurrentIndex - 1 + weapons.Count) % weapons.Count ); }
}