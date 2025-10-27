using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject loadPanel;

    private void Start()
    {
        // Main menu visible by default
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        loadPanel.SetActive(false);
    }

    public void OnStartClick()
    {
        SceneManager.LoadScene("SampleScene");
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
