using UnityEngine;

/// <summary>
/// Standalone audio component for item pickups. Attach to item pickup GameObjects.
/// Plays sound when the GameObject is destroyed (assuming ItemPickup destroys on pickup).
/// </summary>
public class ItemPickupAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private bool playPickupSound = true;
    
    void OnDestroy()
    {
        // Play sound when item is picked up (destroyed)
        if (playPickupSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayItemPickup();
        }
    }
}

