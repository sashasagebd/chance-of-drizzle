using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;


public sealed class Inventory
{
    private static readonly Inventory instance = new Inventory();
    public static Inventory Instance //singleton pattern
    {
        get { return instance; }
    }
    private Inventory()
    {

    }

    private readonly List<InventorySlot> slots = new List<InventorySlot>();

    public event Action InventoryChanged;
    private void OnInventoryChanged()
    {
        InventoryChanged?.Invoke();
    }

    public bool Add(Item item)
    {
        if (item == null)
        {
            return false;
        }
        if (item.CanStack)
        {
            InventorySlot existingSlot = slots.FirstOrDefault(
                s => s.ItemReference.Name == item.Name
            );
            if (existingSlot == null)
            {
                InventorySlot newSlot = new InventorySlot(item, 1);
                slots.Add(newSlot);
                OnInventoryChanged();
                return true;
            }
            else
            {
                existingSlot.Count += 1;
                OnInventoryChanged();
                return true;
            }
        }
        else
        {
            InventorySlot existingSlot = slots.FirstOrDefault(
                s => s.ItemReference.Name == item.Name
            );
            if (existingSlot == null)
            {
                InventorySlot newSlot = new InventorySlot(item, 1);
                slots.Add(newSlot);
                OnInventoryChanged();
                return true;
            }
            else
            {
                //Debug.Log($"Unable to add Item: {item.Name} as it already exists and cannot stack");
                return false;
            }
        }
    }

    public bool Remove(Item item)
    {
        if (item == null)
        {
            UnityEngine.Debug.LogWarning("Attempted to remove a null item.");
            return false;
        }

        InventorySlot existingSlot = slots.FirstOrDefault(
            s => s.ItemReference != null && s.ItemReference.Name == item.Name
        );

        if (existingSlot == null || existingSlot.Count <= 0)
        {
           // UnityEngine.Debug.Log($"{item.Name} cannot be removed because it does not exist in the inventory.");
            return false;
        }

        if (item.CanStack)
        {
            existingSlot.Count -= 1;

            if (existingSlot.Count <= 0)
            {
                slots.Remove(existingSlot);
                UnityEngine.Debug.Log($"Removed last stack of {item.Name}.");
            }
            else
            {
                UnityEngine.Debug.Log($"Removed one {item.Name}. Remaining: {existingSlot.Count}");
            }
        }
        else 
        {
            slots.Remove(existingSlot);
            //UnityEngine.Debug.Log($"Removed unstackable item: {item.Name}.");
        }
        
        OnInventoryChanged(); 
        return true; 
    }
}
