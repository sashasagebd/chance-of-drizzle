using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public Transform iconParent;        // The panel where icons will appear
    public GameObject itemIconPrefab;   // Prefab of the small UI Image

    public void ShowIcon(Sprite iconSprite)
    {
        if (iconParent == null || itemIconPrefab == null || iconSprite == null) return;

        GameObject newIcon = Instantiate(itemIconPrefab, iconParent);
        Image img = newIcon.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = iconSprite;
        }

        newIcon.transform.localScale = Vector3.one;
    }
}
