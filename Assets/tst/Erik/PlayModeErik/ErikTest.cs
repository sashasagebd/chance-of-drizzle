using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/*
public class NewTestScript
{
    
    private GameObject _spawnerObj;
    private ObjectSpawner _spawner;

    private LevelManager _managerScript;
    private ObjectSpawner _spawnerScript;
    private GameObject _visual;

    [OneTimeSetUp]

    public void Setup() 
    {
        _spawnerObj = new GameObject("TestSpawner");
        _spawner = _spawnerObj.AddComponent<ObjectSpawner>();

        _visual = new GameObject("Visual");
        _visual.transform.parent = _spawnerObj.transform;


        _ManagerScript = new LevelManager();
    }


    public IEnumerator Awake_Populates_ValidSpawn_List()
    {
        // add 2 prefabs to spawn list
        var prefabA = new GameObject("A");
        var prefabB = new GameObject("B");

        _spawner.toSpawn.Add(prefabA);
        _spawner.toSpawn.Add(prefabB);

        yield return null; // allow Awake to run

        Assert.IsTrue(
            _spawnerObj == null || _spawner != null,
            "Spawner should not be destroyed since valid objects exist."
        );

        Assert.NotNull(
            _spawner,
            "Spawner should still exist after Awake if valid objects were found."
        );

        // Check internal lists via reflection
        var validSpawn = (System.Collections.Generic.List<GameObject>)
            typeof(ObjectSpawner)
            .GetField("validSpawn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(spawner);

        Assert.AreEqual(2, validSpawn.Count, "validSpawn should contain items that passed SpecificTest.");

        var spawnChoice = typeof(ObjectSpawner)
            .GetField("spawnChoice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(spawner);

        Assert.NotNull(spawnChoice, "spawnChoice should have been randomly selected.");
    }

}
*/