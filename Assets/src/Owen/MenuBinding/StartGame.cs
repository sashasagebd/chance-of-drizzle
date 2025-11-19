using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MenuAction
{
    public override void Execute()
    {
        LogClick();
        SceneManager.LoadScene("LevelFinal");
    }
}
