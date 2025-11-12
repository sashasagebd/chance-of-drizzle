using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
/*
    [Header("References")]
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    [SerializeField] private GameObject pauseMenuPanel; // Assign your Pause Panel in Inspector
    [SerializeField] private MonoBehaviour playerScript; // Drag your player movement/looking script here
=======
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private MonoBehaviour playerScript;
>>>>>>> Stashed changes
=======
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private MonoBehaviour playerScript;
>>>>>>> Stashed changes

    private bool isPaused = false;
    public static bool GameIsPaused = false;

    void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;
        GameIsPaused = true;
        isPaused = true;

        if (playerScript != null)
            playerScript.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        GameIsPaused = false;
        isPaused = false;

        if (playerScript != null)
            playerScript.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Button to go back to Main Menu
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        SceneManager.LoadScene("Menu"); // Replace with your main menu scene name
    }


        public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Button to quit the game
public void QuitGame()
{
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
}

=======
=======
>>>>>>> Stashed changes
        SceneManager.LoadScene("Menu"); // Exact name from Build Settings
    }

    public void RestartGame()
    {
    Time.timeScale = 1f; // Reset time scale
    Scene currentScene = SceneManager.GetActiveScene();
    SceneManager.LoadScene(currentScene.name);
    }

    // Button to quit the game
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes*/
}
