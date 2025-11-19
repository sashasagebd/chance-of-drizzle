using UnityEngine;

public class LoadPanel : MenuAction
{
    public PanelManager panelManager;
    public override void Execute()
    {   
        LogClick();
        if(panelManager != null)
            panelManager.ShowPanel(panelManager.loadPanel);
    }
}
