using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public sealed class Inventory
{
    private static readonly Inventory instance = new Inventory();
    public static Inventory Instance
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
                Debug.Log($"Unable to add Item: {item.Name} as it already exists and cannot stack");
                return false;
            }
        }
    }

    /*public bool Remove(Item item)
    {
        if (item == null)
            return false;
        return items.Remove(item);
    }*/
}
