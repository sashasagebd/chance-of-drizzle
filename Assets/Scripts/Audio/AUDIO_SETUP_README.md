# Audio System Setup Guide

This audio system is designed to work **without modifying other team members' code** to avoid merge conflicts.

## Core Files (Your Audio Files)

All audio scripts are located in: `Assets/Scripts/Audio/`

1. **SoundManager.cs** - Main audio manager singleton
   - Handles all audio playback
   - Supports 3D positional sounds, 2D sounds, music, and ambient audio
   - Assign audio clips in the Unity Inspector

2. **HealthAudio.cs** - Player health audio
   - Attach to player GameObject (requires Health component)
   - Automatically plays death and low health sounds
   - Uses existing `Health.OnDied` event (no code changes needed)

3. **EnemyAudio.cs** - Enemy audio component
   - Attach to enemy GameObjects
   - Provides public methods that can be called via UnityEvents or other scripts
   - Methods: `OnEnemyAttack()`, `OnEnemyHit()`, `OnEnemyDeath()`, `OnEnemyLaser()`, `OnEnemyMissile()`

4. **WeaponAudio.cs** - Weapon audio component
   - Attach to weapon GameObjects
   - Provides public methods for custom weapon classes to call
   - See usage example in the script comments

5. **ItemPickupAudio.cs** - Item pickup audio
   - Attach to item pickup GameObjects
   - Automatically plays sound when item is destroyed (picked up)

6. **UIAudio.cs** - UI button audio
   - Attach to UI Button GameObjects
   - Automatically plays click sounds on button clicks

7. **LevelAudio.cs** - Level/map audio
   - Attach to a GameObject in your scene (like LevelManager)
   - Handles ambient sounds and level transitions

## Setup Instructions

### 1. SoundManager Setup
- Create a GameObject in your scene called "SoundManager"
- Add the `SoundManager` component
- In the Inspector, assign all your audio clips to the organized sections:
  - Weapon Clips
  - Enemy Clips
  - Player Status Clips
  - UI Clips
  - Music & Ambient
  - Map/Level Transition

### 2. Player Audio
- Add `HealthAudio` component to your player GameObject
- Configure low health threshold and cooldown in Inspector

### 3. Enemy Audio (Optional - requires coordination)
Since we can't modify Enemy.cs, you have two options:

**Option A: Use UnityEvents (Recommended)**
- Add `EnemyAudio` component to enemy prefabs
- Use UnityEvents in Inspector to call the audio methods when needed
- Or ask TL5 to add a simple call like: `GetComponent<EnemyAudio>()?.OnEnemyAttack(position);`

**Option B: Create wrapper scripts**
- Create your own enemy wrapper that calls EnemyAudio methods
- This keeps your code separate from TL5's code

### 4. Weapon Audio
Since WeaponBase has virtual methods, create custom weapon classes:

```csharp
// Example: Create this in your own scripts folder
public class AudioProjectileWeapon : ProjectileWeapon 
{
    private WeaponAudio audio;
    
    void Awake() 
    { 
        audio = GetComponent<WeaponAudio>(); 
    }
    
    protected override void OnFired() 
    {
        base.OnFired();
        audio?.OnWeaponFire(transform.position);
    }
    
    protected override void OnReloaded() 
    {
        base.OnReloaded();
        audio?.OnWeaponReload(transform.position);
    }
    
    protected override void OnDryFire() 
    {
        base.OnDryFire();
        audio?.OnWeaponDryFire(transform.position);
    }
}
```

Then use `AudioProjectileWeapon` instead of `ProjectileWeapon` for weapons that need audio.

### 5. UI Audio
- Add `UIAudio` component to all UI Button GameObjects
- It will automatically play click sounds

### 6. Item Pickup Audio
- Add `ItemPickupAudio` component to item pickup prefabs
- Sound plays automatically when item is picked up (destroyed)

### 7. Level Audio
- Add `LevelAudio` component to a GameObject in your scene
- It handles ambient sounds and level transitions automatically

## Available Sound Methods

All methods are accessible via `SoundManager.Instance`:

**Weapon Sounds:**
- `PlayWeaponFire(Vector3 position, float volume)`
- `PlayWeaponReload(Vector3 position, float volume)`
- `PlayWeaponDryFire(Vector3 position, float volume)`
- `PlayHitAt(Vector3 position, float volume)`

**Enemy Sounds:**
- `PlayEnemyAttack(Vector3 position, float volume)`
- `PlayEnemyDeath(Vector3 position, float volume)`
- `PlayEnemyHit(Vector3 position, float volume)`
- `PlayEnemyLaser(Vector3 position, float volume)`
- `PlayEnemyMissile(Vector3 position, float volume)`

**Player Status:**
- `PlayPlayerDamage(float volume)`
- `PlayPlayerHeal(float volume)`
- `PlayPlayerDeath(float volume)`
- `PlayPlayerLowHealth(float volume)`
- `PlayLevelUp(float volume)`
- `PlayItemPickup(float volume)`

**UI:**
- `PlayUIClick(float volume)`
- `PlayUISelect(float volume)`

**Music & Ambient:**
- `StartStageMusic(float volume)`
- `PlayMenuMusic(float volume)`
- `StartAmbient(float volume)`
- `StopAmbient(float fadeOutSeconds)`

**Map/Level:**
- `PlayMapTransition(float volume)`
- `PlayLevelComplete(float volume)`

## Notes

- All audio clips are optional - assign only what you have
- The system gracefully handles null clips (won't crash)
- SoundManager persists across scenes (DontDestroyOnLoad)
- 3D sounds use spatial audio (positional)
- 2D sounds play at full volume regardless of position

