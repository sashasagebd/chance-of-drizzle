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
    
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopAmbient();
            
            if(scene.name.Contains("Level"))
            {
                if (playTransitionSound)
                    SoundManager.Instance.PlayMapTransition();
                if (playAmbientOnStart)
                    SoundManager.Instance.StartAmbient();
            }
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

