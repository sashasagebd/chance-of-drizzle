using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject loadPanel;

    // Awake just calls InitializeMenu if all panels are assigned
    private void Awake()
    {
        if (mainMenuPanel != null && settingsPanel != null && loadPanel != null)
            InitializeMenu();
    }

    public void InitializeMenu()
    {
        // Safety check to avoid NullReferenceException
        if (mainMenuPanel == null || settingsPanel == null || loadPanel == null)
        {
            Debug.LogWarning("MenuController panels not assigned!");
            return;
        }

        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        loadPanel.SetActive(false);
    }

    public void OnStartClick()
    {
        SceneManager.LoadScene("Level_0");
    }

    public void OnSettingsClick()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        loadPanel.SetActive(false);
    }

    public void OnLoadClick()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        loadPanel.SetActive(true);
    }

    public void OnBackClick()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        loadPanel.SetActive(false);
    }

    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
