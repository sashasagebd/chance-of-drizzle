using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class StartGame : MenuAction
{

    [SerializeField] protected List<string> startDestinations = new List<string>();

    public override void Execute()
    {
        LogClick();
        string startDestination = startDestinations[Random.Range(0,startDestinations.Count)];
        SceneManager.LoadScene(startDestination);
    }
}
