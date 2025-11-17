Prefab: LevelSoundManager

A persistent audio controller that automatically plays ambient audio and level-transition SFX whenever a new scene loads.

How to Spawn
Place LevelSoundManager in the first scene along with SoundManager.
It uses DontDestroyOnLoad, so only one instance should exist.
It should not be spawned manually during gameplay and must exist before level loading begins.

Requirements
Requires SoundManager to already be present
Subscribes to SceneManager.sceneLoaded
Should be initialized in Start(), not Awake()

What It Does
Automatically handles:
Ambient loop playback when a level begins
Transition sound when a scene loads
Ambient stop/restart across levels
Level-based audio rules (ex: only play ambience on scenes with “Level” in the name)

Inspector booleans:
playAmbientOnStart
playTransitionSound
