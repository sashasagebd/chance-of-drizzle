using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float volume = 0.7f;
    private AudioSource _source;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _source = gameObject.AddComponent<AudioSource>();
        _source.loop = true;
        _source.playOnAwake = false;
        _source.volume = volume;
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            _source.clip = backgroundMusic;
            _source.Play();
        }
    }

    // Optional: Use to swap tracks at runtime
    public void PlayMusic(AudioClip newClip, float newVolume = 0.7f)
    {
        if (newClip == null) return;
        _source.clip = newClip;
        _source.volume = newVolume;
        _source.Play();
    }
}
