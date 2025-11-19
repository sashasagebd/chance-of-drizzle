using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    [SerializeField] private Light flickerLight;
    [SerializeField] private float minIntensity = 2f;
    [SerializeField] private float maxIntensity = 5f;
    [SerializeField] private float flickerSpeed = 0.05f;

    void Update()
    {
        flickerLight.intensity = Random.Range(minIntensity, maxIntensity);
    }
}
