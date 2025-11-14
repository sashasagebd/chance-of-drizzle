using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MenuControllerStressTests
{
    private MenuController menuController;
    private GameObject mainMenuPanel, settingsPanel, loadPanel;

    [UnityTest]
    public IEnumerator StressTest_UntilCrash()
    {
        // Setup panels and controller
        mainMenuPanel = new GameObject("MainMenuPanel");
        settingsPanel = new GameObject("SettingsPanel");
        loadPanel = new GameObject("LoadPanel");

        var go = new GameObject("MenuController");
        menuController = go.AddComponent<MenuController>();

        menuController.mainMenuPanel = mainMenuPanel;
        menuController.settingsPanel = settingsPanel;
        menuController.loadPanel = loadPanel;

        menuController.InitializeMenu();
        yield return null;

        int count = 0;

        // Loop indefinitely until an exception occurs
        while (true)
        {
            try
            {
                menuController.OnSettingsClick();
                menuController.OnBackClick();
                menuController.OnLoadClick();
                menuController.OnBackClick();

                count++;

                // Log every 100 iterations to monitor progress
                if (count % 100 == 0)
                    Debug.Log($"Stress iteration: {count} | Active panel: " +
                              (mainMenuPanel.activeSelf ? "Main" :
                              settingsPanel.activeSelf ? "Settings" : "Load"));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Crash at iteration {count}: {ex.Message}");
                Assert.Fail($"Test crashed at iteration {count}: {ex}");
                yield break;
            }

            yield return null; // allow Unity frame update
        }
    }
}
