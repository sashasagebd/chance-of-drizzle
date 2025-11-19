using UnityEngine;
using System.Collections;

public class LightningStorm : MonoBehaviour
{
    [Header("Lightning Settings")]
    [SerializeField] private Light lightningLight;              
    [SerializeField] private float minInterval = 5f;           
    [SerializeField] private float maxInterval = 15f;           
    [SerializeField] private int minFlashesPerBolt = 1;        
    [SerializeField] private int maxFlashesPerBolt = 4;       
    [SerializeField] private float flashDuration = 0.08f;       
    [SerializeField] private float flashDelay = 0.1f;          
    [SerializeField] private float minFlashIntensity = 2f;     
    [SerializeField] private float maxFlashIntensity = 5f;    

    [Header("Camera Shake Settings")]
    [SerializeField] private CameraShake cameraShake;         

    [Header("Audio Settings")]
    [SerializeField] private AudioSource thunderAudio;         
    [SerializeField] private AudioClip[] thunderClips;         
    [SerializeField] private float minThunderDelay = 0.1f;    
    [SerializeField] private float maxThunderDelay = 0.5f;    

    private float nextFlashTime;

    void Start()
    {
        if (lightningLight == null) return;
        lightningLight.intensity = 0;
        ScheduleNextStrike();
    }

    void Update()
    {
        if (Time.time >= nextFlashTime)
        {
            StartCoroutine(Strike()); //Iterator stepping through patterns
            ScheduleNextStrike();
        }
    }

    void ScheduleNextStrike()
    {
        nextFlashTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    //Iterator strike

    IEnumerator Strike()
    {
        int flashes = Random.Range(minFlashesPerBolt, maxFlashesPerBolt + 1);

        for (int i = 0; i < flashes; i++)
        {
       //flash amount
            float randomIntensity = Random.Range(minFlashIntensity, maxFlashIntensity);
            lightningLight.intensity = randomIntensity;

         //shake
            if (cameraShake != null)
                cameraShake.Shake();

          //audio selection 
            if (i == 0 && thunderAudio != null && thunderClips.Length > 0)
            {
                float delay = Random.Range(minThunderDelay, maxThunderDelay);
                StartCoroutine(PlayThunderWithDelay(delay));
            }

            yield return new WaitForSeconds(flashDuration);

            lightningLight.intensity = 0;

       
            if (i < flashes - 1)
                yield return new WaitForSeconds(flashDelay * Random.Range(0.8f, 1.2f));
        }
    }

    IEnumerator PlayThunderWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (thunderAudio == null || thunderClips.Length == 0)
            yield break;

     
        int index = Random.Range(0, thunderClips.Length);
        thunderAudio.clip = thunderClips[index];

  
        thunderAudio.volume = Random.Range(0.7f, 1f);
        thunderAudio.pitch = Random.Range(0.95f, 1.05f);

        thunderAudio.Play();
    }
}
