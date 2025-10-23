using UnityEngine;
using System;

public abstract class Item
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool CanStack { get; set; } = true;
    



    public abstract void Use(object target);
    public virtual void ShowItem()
    {
        Debug.Log($"This is: {Name}, {Description}");
    }
    

}