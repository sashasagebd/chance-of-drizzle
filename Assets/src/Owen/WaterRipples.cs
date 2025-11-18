using UnityEngine;

public class WaterRipplesOscillating : MonoBehaviour
{
    public Material waterMaterial;          
    public Vector2 amplitude = new Vector2(0.02f, 0.02f);
    public Vector2 speed = new Vector2(1f, 1f);         

    private Vector2 offset;

    void Update()
    {
        offset.x = Mathf.Sin(Time.time * speed.x) * amplitude.x;
        offset.y = Mathf.Sin(Time.time * speed.y) * amplitude.y;

        waterMaterial.SetTextureOffset("_BaseMap", offset);
    }
}
