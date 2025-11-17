The SoundManager controls all game audio, including:
3D positional sound effects
2D UI sounds
Music playback
Ambient environmental audio
It uses a singleton and persists across scenes using DontDestroyOnLoad.
- It Should Be Spawned by 
Drag SoundManager.prefab into the first scene of the game.
It will automatically persist between scenes.
Do not add duplicates in other scenes.
- It Is Required
Required for any script that plays audio (weapons, enemies, UI, items).
Must exist before any level loads; otherwise, audio calls will return null.
Required by LevelSoundManager.
