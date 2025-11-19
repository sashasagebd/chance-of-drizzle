using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject loadPanel;

    public void ShowPanel(GameObject panelToShow)
    {
        mainMenuPanel.SetActive(panelToShow == mainMenuPanel);
        settingsPanel.SetActive(panelToShow == settingsPanel);
        loadPanel.SetActive(panelToShow == loadPanel);
    }
}
