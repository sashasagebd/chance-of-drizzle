using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MenuControllerPlayModeTests
{
    private MenuController menuController;
    private GameObject mainMenuPanel, settingsPanel, loadPanel;

    [SetUp]
    public void Setup()
    {
        mainMenuPanel = new GameObject("MainMenuPanel");
        settingsPanel = new GameObject("SettingsPanel");
        loadPanel = new GameObject("LoadPanel");

        var go = new GameObject("MenuController");
        menuController = go.AddComponent<MenuController>();

        menuController.mainMenuPanel = mainMenuPanel;
        menuController.settingsPanel = settingsPanel;
        menuController.loadPanel = loadPanel;

        menuController.InitializeMenu();
    }

    // =========================
    // Functional Tests
    // =========================

    [UnityTest]
    public IEnumerator Functional_OnlyMainActive()
    {
        yield return null;
        Assert.IsTrue(mainMenuPanel.activeSelf);
        Assert.IsFalse(settingsPanel.activeSelf);
        Assert.IsFalse(loadPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator Functional_ActivateSettings()
    {
        menuController.OnSettingsClick();
        yield return null;

        Assert.IsFalse(mainMenuPanel.activeSelf);
        Assert.IsTrue(settingsPanel.activeSelf);
        Assert.IsFalse(loadPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator Functional_ActivateLoad()
    {
        menuController.OnLoadClick();
        yield return null;

        Assert.IsFalse(mainMenuPanel.activeSelf);
        Assert.IsFalse(settingsPanel.activeSelf);
        Assert.IsTrue(loadPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator Functional_BackFromSettings()
    {
        menuController.OnSettingsClick();
        yield return null;

        menuController.OnBackClick();
        yield return null;

        Assert.IsTrue(mainMenuPanel.activeSelf);
        Assert.IsFalse(settingsPanel.activeSelf);
        Assert.IsFalse(loadPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator Functional_SequentialSwitching()
    {
        menuController.OnSettingsClick();
        yield return null;

        menuController.OnLoadClick();
        yield return null;

        Assert.IsFalse(mainMenuPanel.activeSelf);
        Assert.IsFalse(settingsPanel.activeSelf);
        Assert.IsTrue(loadPanel.activeSelf);
    }

    // =========================
    // Stress / Repeated Interaction
    // =========================

    [UnityTest]
    public IEnumerator Stress_RapidSwitching100Times()
    {
        for (int i = 0; i < 100; i++)
        {
            menuController.OnSettingsClick();
            menuController.OnLoadClick();
            menuController.OnBackClick();
            yield return null;
        }

        Assert.IsTrue(mainMenuPanel.activeSelf);
        Assert.IsFalse(settingsPanel.activeSelf);
        Assert.IsFalse(loadPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator Stress_RapidSettingsClick()
    {
        for (int i = 0; i < 50; i++)
        {
            menuController.OnSettingsClick();
            yield return null;
        }

        Assert.IsTrue(settingsPanel.activeSelf);
        Assert.IsFalse(mainMenuPanel.activeSelf);
        Assert.IsFalse(loadPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator Stress_RapidLoadClick()
    {
        for (int i = 0; i < 50; i++)
        {
            menuController.OnLoadClick();
            yield return null;
        }

        Assert.IsTrue(loadPanel.activeSelf);
        Assert.IsFalse(mainMenuPanel.activeSelf);
        Assert.IsFalse(settingsPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator Stress_BackRepeatedly()
    {
        for (int i = 0; i < 50; i++)
        {
            menuController.OnBackClick();
            yield return null;
        }

        Assert.IsTrue(mainMenuPanel.activeSelf);
        Assert.IsFalse(settingsPanel.activeSelf);
        Assert.IsFalse(loadPanel.activeSelf);
    }

    // =========================
    // Boundary / Null Handling
    // =========================

 [UnityTest]
public IEnumerator Functional_SettingsLoadSettings()
{
    menuController.OnSettingsClick();
    yield return null;

    menuController.OnLoadClick();
    yield return null;

    menuController.OnSettingsClick();
    yield return null;

    Assert.IsFalse(mainMenuPanel.activeSelf, "Main menu should be inactive.");
    Assert.IsTrue(settingsPanel.activeSelf, "Settings panel should be active.");
    Assert.IsFalse(loadPanel.activeSelf, "Load panel should be inactive.");
}

[UnityTest]
public IEnumerator Functional_BackLoadBack()
{
    menuController.OnBackClick();
    yield return null;

    menuController.OnLoadClick();
    yield return null;

    menuController.OnBackClick();
    yield return null;

    Assert.IsTrue(mainMenuPanel.activeSelf, "Main menu should be active.");
    Assert.IsFalse(settingsPanel.activeSelf, "Settings panel should be inactive.");
    Assert.IsFalse(loadPanel.activeSelf, "Load panel should be inactive.");
}

[UnityTest]
public IEnumerator Stress_RepeatedFullCycle10Times()
{
    for (int i = 0; i < 10; i++)
    {
        menuController.OnSettingsClick();
        yield return null;
        menuController.OnLoadClick();
        yield return null;
        menuController.OnBackClick();
        yield return null;
    }

    Assert.IsTrue(mainMenuPanel.activeSelf, "Main menu should be active after repeated cycles.");
    Assert.IsFalse(settingsPanel.activeSelf, "Settings panel should be inactive.");
    Assert.IsFalse(loadPanel.activeSelf, "Load panel should be inactive.");
}

[UnityTest]
public IEnumerator Functional_OnlyOnePanelActiveAtATime()
{
    menuController.OnSettingsClick();
    yield return null;
    Assert.AreEqual(1, CountActivePanels(), "Only one panel should be active.");

    menuController.OnLoadClick();
    yield return null;
    Assert.AreEqual(1, CountActivePanels(), "Only one panel should be active.");

    menuController.OnBackClick();
    yield return null;
    Assert.AreEqual(1, CountActivePanels(), "Only one panel should be active.");
}

private int CountActivePanels()
{
    int count = 0;
    if (mainMenuPanel.activeSelf) count++;
    if (settingsPanel.activeSelf) count++;
    if (loadPanel.activeSelf) count++;
    return count;
}


    // =========================
    // Combined Edge Case
    // =========================

    [UnityTest]
    public IEnumerator Negative_RepeatedFullCycle()
    {
        for (int i = 0; i < 10; i++)
        {
            menuController.OnSettingsClick();
            menuController.OnLoadClick();
            menuController.OnBackClick();
            yield return null;
        }

        Assert.IsTrue(mainMenuPanel.activeSelf);
        Assert.IsFalse(settingsPanel.activeSelf);
        Assert.IsFalse(loadPanel.activeSelf);
    }

    [UnityTest]
    public IEnumerator Stress_UntilCrashSimulation()
    {
        int count = 0;

        while (count < 500) // simulate stress without infinite loop
        {
            menuController.OnSettingsClick();
            menuController.OnLoadClick();
            menuController.OnBackClick();
            yield return null;
            count++;
        }

        Assert.IsTrue(mainMenuPanel.activeSelf);
    }
}
