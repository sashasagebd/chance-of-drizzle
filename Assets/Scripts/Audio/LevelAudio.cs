using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Standalone audio component for level/map transitions.
/// Attach to a GameObject in your scene (like LevelManager) to handle level audio.
/// </summary>
public class LevelAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private bool playAmbientOnStart = true;
    [SerializeField] private bool playTransitionSound = true;
    
    static LevelAudio _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (_instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[LevelAudio] Scene Loaded: {scene.name}");
        if (SoundManager.Instance != null)
        {
             Debug.Log($"[LevelAudio] SoundManager exists: {SoundManager.Instance.name}");

            SoundManager.Instance.StopAmbient(0f);
            
            if(scene.name.Contains("Level"))
            {
                if (playTransitionSound)
                    SoundManager.Instance.PlayMapTransition();
                if (playAmbientOnStart)
                    SoundManager.Instance.StartAmbient();
            }
        }
        else
        {
            Debug.LogError("[LevelAudio] SoundManager.Instance is NULL!");
        }
    }
    
    // Can be called manually for level completion
    public void OnLevelComplete()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLevelComplete();
        }
    }
}

