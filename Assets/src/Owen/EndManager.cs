using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    [Header("UI Panels (Optional)")]
    [SerializeField] private GameObject winPanel;

    void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Called by "Main Menu" button
    public void OnMainMenuClick()
    {
        SceneManager.LoadScene("Menu");
    }

    // Called by "Quit" button
    public void OnQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("Quitting game...");
    }
}
