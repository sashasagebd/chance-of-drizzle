using UnityEngine;
using System;

public abstract class Item
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool CanStack { get; set; } = true;


    public abstract void Use(object target);
    public virtual void TriggerVisualEffects(object target) //virtual method
    {
        if (target is PlayerController3D player)
        {
            if(VisualEffects.Instance != null)
            {
                VisualEffects.Instance.PlayVisual("Puff_HighPerformance", player.transform.position);
            }
        }
    }
    public void ShowItem()
    {
        Debug.Log($"This is: {Name}, {Description}");
    }
    

}