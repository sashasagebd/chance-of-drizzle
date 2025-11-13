using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Standalone audio component for UI elements. Attach to UI buttons or panels.
/// Automatically plays click sounds on button clicks without modifying MenuController.
/// </summary>
public class UIAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private bool playSelectSound = false;
    
    private Button _button;
    
    void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(OnButtonClick);
        }
    }
    
    void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnButtonClick);
        }
    }
    
    void OnButtonClick()
    {
        if (playClickSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIClick();
        }
    }
    
    // Can be called from UnityEvents or other scripts
    public void PlayClickSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIClick();
        }
    }
    
    public void PlaySelectSound()
    {
        if (playSelectSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUISelect();
        }
    }
}

