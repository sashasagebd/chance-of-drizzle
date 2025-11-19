using NUnit.Framework;
using UnityEngine;

public class MenuControllerEditModeTests
{
    private MenuController _menuController;
    private GameObject _mainMenuPanel;
    private GameObject _settingsPanel;
    private GameObject _loadPanel;

    [SetUp]
    public void Setup()
    {
        _mainMenuPanel = new GameObject("MainMenuPanel");
        _settingsPanel = new GameObject("SettingsPanel");
        _loadPanel = new GameObject("LoadPanel");

        var controllerObject = new GameObject("MenuController");
        _menuController = controllerObject.AddComponent<MenuController>();

        _menuController.mainMenuPanel = _mainMenuPanel;
        _menuController.settingsPanel = _settingsPanel;
        _menuController.loadPanel = _loadPanel;

        _menuController.InitializeMenu();
    }

    // =========================
    // Functional Tests
    // =========================

    [Test]
    public void Functional_OnlyMainActive()
    {
        Assert.IsTrue(_mainMenuPanel.activeSelf, "Main menu should be active.");
        Assert.IsFalse(_settingsPanel.activeSelf, "Settings panel should be inactive.");
        Assert.IsFalse(_loadPanel.activeSelf, "Load panel should be inactive.");
    }

    [Test]
    public void Functional_ActivatesSettingsPanel()
    {
        _menuController.OnSettingsClick();

        Assert.IsFalse(_mainMenuPanel.activeSelf, "Main menu should be inactive after SettingsClick.");
        Assert.IsTrue(_settingsPanel.activeSelf, "Settings panel should be active after SettingsClick.");
        Assert.IsFalse(_loadPanel.activeSelf, "Load panel should be inactive after SettingsClick.");
    }

    [Test]
    public void Functional_ActivatesLoadPanel()
    {
        _menuController.OnLoadClick();

        Assert.IsFalse(_mainMenuPanel.activeSelf, "Main menu should be inactive after LoadClick.");
        Assert.IsFalse(_settingsPanel.activeSelf, "Settings panel should be inactive after LoadClick.");
        Assert.IsTrue(_loadPanel.activeSelf, "Load panel should be active after LoadClick.");
    }

    [Test]
    public void Functional_ReturnsToMainMenu()
    {
        _menuController.OnSettingsClick();
        _menuController.OnBackClick();

        Assert.IsTrue(_mainMenuPanel.activeSelf, "Main menu should be active after BackClick.");
        Assert.IsFalse(_settingsPanel.activeSelf, "Settings panel should be inactive after BackClick.");
        Assert.IsFalse(_loadPanel.activeSelf, "Load panel should be inactive after BackClick.");
    }

    [Test]
    public void Functional_MultipleSettingsClicks()
    {
        _menuController.OnSettingsClick();
        _menuController.OnSettingsClick(); // clicking again
        Assert.IsTrue(_settingsPanel.activeSelf);
        Assert.IsFalse(_mainMenuPanel.activeSelf);
        Assert.IsFalse(_loadPanel.activeSelf);
    }

    [Test]
    public void Functional_SequentialPanelSwitch()
    {
        _menuController.OnSettingsClick();
        _menuController.OnLoadClick();
        Assert.IsFalse(_mainMenuPanel.activeSelf);
        Assert.IsFalse(_settingsPanel.activeSelf);
        Assert.IsTrue(_loadPanel.activeSelf);
    }

    // =========================
    // Boundary Tests
    // =========================

    [Test]
    public void Boundary_PanelsDontCrash()
    {
        var controllerObject = new GameObject("MenuControllerBoundary");
        var menu = controllerObject.AddComponent<MenuController>();

        menu.mainMenuPanel = null;
        menu.settingsPanel = null;
        menu.loadPanel = null;

        menu.InitializeMenu();

        Assert.Pass("Null panels do not crash the controller.");
    }

    [Test]
    public void Boundary_MultipleActivePanels()
    {
        var mainMenuPanel = new GameObject("MainMenuPanel");
        var settingsPanel = new GameObject("SettingsPanel");
        var loadPanel = new GameObject("LoadPanel");

        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(true);
        loadPanel.SetActive(true);

        var controllerObject = new GameObject("MenuController");
        var menu = controllerObject.AddComponent<MenuController>();
        menu.mainMenuPanel = mainMenuPanel;
        menu.settingsPanel = settingsPanel;
        menu.loadPanel = loadPanel;

        menu.OnSettingsClick();
        Assert.IsFalse(mainMenuPanel.activeSelf);
        Assert.IsTrue(settingsPanel.activeSelf);
        Assert.IsFalse(loadPanel.activeSelf);

        menu.OnLoadClick();
        Assert.IsFalse(mainMenuPanel.activeSelf);
        Assert.IsFalse(settingsPanel.activeSelf);
        Assert.IsTrue(loadPanel.activeSelf);

        menu.OnBackClick();
        Assert.IsTrue(mainMenuPanel.activeSelf);
        Assert.IsFalse(settingsPanel.activeSelf);
        Assert.IsFalse(loadPanel.activeSelf);
    }

    [Test]
    public void Boundary_BackOnMainDoesNothing()
    {
        _menuController.OnBackClick();
        Assert.IsTrue(_mainMenuPanel.activeSelf);
        Assert.IsFalse(_settingsPanel.activeSelf);
        Assert.IsFalse(_loadPanel.activeSelf);
    }

    [Test]
    public void Functional_MultipleBackFromLoad()
    {
    _menuController.OnLoadClick();
    _menuController.OnBackClick();
    _menuController.OnBackClick();
    _menuController.OnBackClick();

    Assert.IsTrue(_mainMenuPanel.activeSelf, "Main menu should stay active.");
    Assert.IsFalse(_settingsPanel.activeSelf, "Settings panel should remain inactive.");
    Assert.IsFalse(_loadPanel.activeSelf, "Load panel should remain inactive.");
    }

    [Test]
    public void Boundary_MultipleBackClicks()
    {
        _menuController.OnSettingsClick();
        _menuController.OnBackClick();
        _menuController.OnBackClick(); // clicking again
        Assert.IsTrue(_mainMenuPanel.activeSelf);
        Assert.IsFalse(_settingsPanel.activeSelf);
        Assert.IsFalse(_loadPanel.activeSelf);
    }

    // =========================
    // Negative / Edge Case Tests
    // =========================

    [Test]
    public void Negative_AssignInvalidPanels()
    {
        _menuController.mainMenuPanel = null;
        _menuController.settingsPanel = null;
        _menuController.loadPanel = null;
        Assert.DoesNotThrow(() => _menuController.InitializeMenu());
    }

    [Test]
    public void Negative_RepeatedSettingsClick()
    {
        _menuController.OnSettingsClick();
        _menuController.OnSettingsClick();
        _menuController.OnSettingsClick();
        Assert.IsTrue(_settingsPanel.activeSelf);
    }

    [Test]
    public void Negative_RepeatedLoadClick()
    {
        _menuController.OnLoadClick();
        _menuController.OnLoadClick();
        Assert.IsTrue(_loadPanel.activeSelf);
    }

    [Test]
    public void Negative_BackAfterMultipleSwitches()
    {
        _menuController.OnSettingsClick();
        _menuController.OnLoadClick();
        _menuController.OnBackClick();
        Assert.IsTrue(_mainMenuPanel.activeSelf);
        Assert.IsFalse(_settingsPanel.activeSelf);
        Assert.IsFalse(_loadPanel.activeSelf);
    }
}
