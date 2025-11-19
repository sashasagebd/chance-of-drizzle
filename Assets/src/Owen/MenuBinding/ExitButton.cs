using UnityEngine;

public class ExitButton : MenuAction
{
    public override void Execute()
    {
        LogClick();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
        
    }
}
