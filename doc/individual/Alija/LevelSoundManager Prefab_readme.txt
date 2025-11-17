The LevelSoundManager automatically handles:
Ambient loops when levels start
Transition SFX when scenes load
Scene-based sound behavior via SceneManager.sceneLoaded
It persists between scenes and depends on SoundManager.
It Should Be Spawned :
Add LevelSoundManager.prefab to the first scene.
It persists via DontDestroyOnLoad.
Only one instance should exist.
It Is Required :
Required in all levels where ambient or transition audio should play.
Requires SoundManager to already be loaded in the scene.
