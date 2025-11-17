Prefab : Sound Manager 
A global audio system that handles all game sound effects, UI sounds, music, and ambient loops.
The SoundManager is a singleton and persists across scenes using DontDestroyOnLoad.
-How to Use
Dragging the SoundManager prefab into a scene will create a working instance, but only one instance should ever exist.
It must be placed in the first scene of the game so it persists for the rest of the playthrough.

All audio playback is triggered through static calls such as:
SoundManager.Instance.PlayWeaponFire(Vector3 position);
SoundManager.Instance.PlayEnemyAttack(Vector3 position);
SoundManager.Instance.PlayUIClick();
SoundManager.Instance.StartStageMusic();

Requirements
The SoundManager must load before any scripts try to play sound.

This means:
It should be added in Start Scene / Main Menu Scene
Other scripts should call SoundManager in Start(), not Awake()
If SoundManager is missing, all audio calls safely do nothing (null-checked), but no sounds will play.

Inspector Configuration
The prefab contains organized audio clip sections:
Weapon Clips (fire, reload, dry fire, hit)
Enemy Clips (attack, hit, death, missile, laser)
Player Status Clips (damage, heal, death, low health, level up, pickup)
UI Clips (click, select)
Music & Ambient (menu music, stage loop, ambient loop)
Map/Level Transition (transition, level complete)
Assign the audio files through the Inspector before gameplay.
Components

The SoundManager prefab includes:
SoundManager.cs – controls all playback
Internal pooled AudioSource objects for:
3D spatial effects
2D UI/audio notifications
looping ambience/music

What the Script Does
Provides a single, globally accessible sound system
Pools audio sources to avoid performance spikes
Plays 3D and 2D sounds
Fades music and ambient loops
Persists across scenes
Prevents duplicate managers from spawning
