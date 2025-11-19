using UnityEngine;

public class BackToMainMenu : MenuAction
{
    public PanelManager panelManager;
    public override void Execute()
    {
        LogClick();
        if(panelManager != null)
            panelManager.ShowPanel(panelManager.mainMenuPanel);
    }
}
