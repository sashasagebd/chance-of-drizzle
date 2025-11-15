using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    public Light flickerLight; // Assign your point/spot light
    public float minIntensity = 2f;
    public float maxIntensity = 5f;
    public float flickerSpeed = 0.05f;

    void Update()
    {
        flickerLight.intensity = Random.Range(minIntensity, maxIntensity);
    }
}
