using UnityEngine;

public class OpenSettings : MenuAction
{
    public PanelManager panelManager;

    public override void Execute()
    {
        LogClick();
        if(panelManager != null)
            panelManager.ShowPanel(panelManager.settingsPanel);
    }
}
