using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    [Header("Mixer Reference")]
    [SerializeField] private AudioMixer mixer;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        // save and defaults
        masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 20f);
        musicSlider.value  = PlayerPrefs.GetFloat("MusicVol", 20f);
        sfxSlider.value    = PlayerPrefs.GetFloat("SFXVol", 20f);

        // Apply to mixer
        SetMaster(masterSlider.value);
        SetMusic(musicSlider.value);
        SetSFX(sfxSlider.value);

        masterSlider.onValueChanged.AddListener(SetMaster);
        musicSlider.onValueChanged.AddListener(SetMusic);
        sfxSlider.onValueChanged.AddListener(SetSFX);
    }

    public void SetMaster(float value)
    {
        mixer.SetFloat("MasterVol", value);
        PlayerPrefs.SetFloat("MasterVol", value);
    }

    public void SetMusic(float value)
    {
        mixer.SetFloat("MusicVol", value);
        PlayerPrefs.SetFloat("MusicVol", value);
    }

    public void SetSFX(float value)
    {
        mixer.SetFloat("SFXVol", value);
        PlayerPrefs.SetFloat("SFXVol", value);
    }
}
