using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDManager : MonoBehaviour
{
    public Transform iconParent;       
    public GameObject itemIconPrefab;  

    public void ShowIcon(Sprite iconSprite, float duration = 0f)
    {
        if (iconParent == null || itemIconPrefab == null || iconSprite == null) return;

        GameObject newIcon = Instantiate(itemIconPrefab, iconParent);
        Image img = newIcon.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = iconSprite;
        }

        newIcon.transform.localScale = Vector3.one;

    
        if (duration > 0)
        {
            StartCoroutine(RemoveAfterDelay(newIcon, duration));
        }
    }

    private IEnumerator RemoveAfterDelay(GameObject icon, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(icon);
    }
}
