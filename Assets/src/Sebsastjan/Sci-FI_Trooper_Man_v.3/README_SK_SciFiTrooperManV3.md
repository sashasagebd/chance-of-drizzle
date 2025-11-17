# SK_SciFiTrooperManV3 Prefab Documentation

## Overview

**SK_SciFiTrooperManV3** is the fully-featured player character prefab for *Chance of Drizzle*, a Risk of Rain-style 3D roguelike shooter. This prefab combines a sci-fi trooper character model with advanced animation systems, weapon handling, player movement, health management, and particle effects.

**Location:** `Assets/src/Sebsastjan/Sci-FI_Trooper_Man_v.3/Prefabs/SK_SciFiTrooperManV3.prefab`

**Model Source:** `Assets/src/Sebsastjan/Sci-FI_Trooper_Man_v.3/Meshes/SK_SciFiTrooperManV3.fbx`

---

## Table of Contents

1. [Prefab Structure](#prefab-structure)
2. [Core Components](#core-components)
3. [Animation System](#animation-system)
4. [Weapon System](#weapon-system)
5. [Movement & Controls](#movement--controls)
6. [Health System](#health-system)
7. [Visual Effects](#visual-effects)
8. [Configuration Guide](#configuration-guide)
9. [Usage Instructions](#usage-instructions)
10. [Technical Details](#technical-details)

---

## Prefab Structure

The prefab has a hierarchical structure with the following key components:

```
SK_SciFiTrooperManV3 (Root)
├── CameraFollow (Camera target)
├── WeaponPivot (Weapon inventory & weapons)
│   ├── Weapon1 (ProjectileWeapon or LazerWeapon)
│   ├── Weapon2
│   └── HitEffect (Particle system for hits)
│       ├── Sparks
│       ├── Decal
│       └── Dust
├── RigLayer_BodyAim (Animation rigging)
│   ├── AimSpine1
│   ├── AimSpine2
│   └── AimHead
├── RigLayer_WeaponAiming (Weapon aim rigging)
│   └── AimSpine2
├── RigLayer_WeaponPose (Weapon pose rigging)
│   ├── WeaponPose
│   ├── RightHandK (Right hand IK)
│   └── RaycastOrigin
├── RigLayer_HandIK (Hand IK rigging)
│   ├── LeftHandK (Left hand IK)
│   └── Muzzle
└── [Character Mesh & Skeleton]
```

**Layer:** Player layer (Layer 3)

---

## Core Components

### 1. PlayerController3D

**Script:** `Assets/Scripts/Sebastjan_TL_2/Player/PlayerController3D.cs`

The main player controller that handles movement, input, weapon management, and item buffs.

#### Key Features:
- **Movement States:** Idle, Walk, Run, Sprint, Crouch, Jump
- **Camera-Relative Movement:** Movement is relative to camera direction
- **Smooth Crouching:** Transitions smoothly between standing and crouching heights
- **Item System Integration:** Supports speed buffs, jump boosts, armor, and damage bonuses
- **Unity Input System:** Uses the new Input System for modern input handling

#### Inspector Settings:

| Setting | Default | Description |
|---------|---------|-------------|
| `maxSpeed` | 15.0 | Maximum movement speed cap |
| `runSpeed` | 4.0 | Base running speed |
| `sprintSpeed` | 9.0 | Sprint speed (Shift key) |
| `crouchSpeed` | 2.0 | Movement speed while crouching |
| `jumpSpeed` | 5.5 | Vertical jump velocity |
| `gravity` | -9.81 | Gravity acceleration |
| `standingHeight` | 2.0 | CharacterController height when standing |
| `crouchingHeight` | 1.0 | CharacterController height when crouching |
| `crouchTransitionSpeed` | 10.0 | Smoothing speed for crouch transitions |

#### References:
- `cam` - Main camera transform (for camera-relative movement)
- `inventory` - WeaponInventory component on WeaponPivot
- `muzzle` - Fallback muzzle transform if weapon doesn't have one
- `animationController` - PlayerAnimationController for animation updates
- `characterAiming` - CharacterAiming for aiming rig control
- `HealthComponent` - Health component reference
- `WeaponComponent` - Current weapon reference for item system

#### Item System Properties:
- `baseDefense` - Base defense value before armor
- `currentDefense` - Current total defense (base + armor)
- `equippedArmor` - Dictionary of equipped armor by type
- `damageBonus` - Static damage bonus from items

#### Public Methods:

```csharp
// Apply speed buff (temporary or permanent)
void ApplySpeed(float amount, int duration)

// Apply jump boost (temporary or permanent)
void ApplyJumpBoost(float amount, int duration)

// Equip armor piece (returns true if equipped, outputs replaced armor)
bool EquipArmor(Armor newArmor, out Armor replacedArmor)
```

---

### 2. Health

**Script:** `Assets/Scripts/Sebastjan_TL_2/Player/Health.cs`

Manages player health, damage, healing, and death.

#### Inspector Settings:

| Setting | Default | Description |
|---------|---------|-------------|
| `maxHp` | 100.0 | Maximum health points |
| `destroyOnDeath` | false | Whether to destroy GameObject on death |

#### Public Properties:
- `Current` - Current health value (read-only)

#### Events:
- `OnDied` - Invoked when health reaches zero
- `OnHealthChanged(float current, float max)` - Invoked when health changes

#### References:
- `healthHUD` - HealthHUD component for UI updates

#### Public Methods:

```csharp
// Apply damage (reduced by armor)
void ApplyDamage(float amount)

// Heal player
void Heal(int amount)

// Increase maximum health
void IncreaseMaxHealth(int amount)

// Set health to specific value
void SetHealth(int value)
```

#### Armor System:
Damage is reduced by the player's `currentDefense` value:
```
damageTaken = amount * (1 - defense)
```
Defense is clamped between 0 and 1 (0% to 100% reduction).

---

### 3. PlayerAnimationController

**Script:** `Assets/Scripts/Sebastjan_TL_2/Player/PlayerAnimationController.cs`

Advanced animation controller with 8-way locomotion blend tree support.

#### Features:
- 8-directional movement blending
- Smooth velocity transitions
- Sprint/crouch state handling
- Jump/landing detection
- Shooting animation integration
- Aiming pose control

#### Inspector Settings:

| Setting | Default | Description |
|---------|---------|-------------|
| `velocityDampTime` | 0.1 | Smoothing speed for velocity parameters |
| `idleSpeedThreshold` | 0.1 | Speed threshold to trigger idle animation |
| `shootAnimationDuration` | 0.3 | Duration of shoot animation overlay |
| `aimWhenShooting` | true | Use aiming pose when shooting |
| `runAnimationSpeed` | 1.25 | Animation speed multiplier for running |
| `sprintAnimationSpeed` | 1.0 | Animation speed multiplier for sprinting |

#### Animator Parameters:

| Parameter | Type | Description |
|-----------|------|-------------|
| `VelocityX` | Float | Normalized lateral velocity (-1 to 1) |
| `VelocityZ` | Float | Normalized forward velocity (-1 to 1) |
| `Speed` | Float | Horizontal speed magnitude |
| `isGrounded` | Bool | Whether character is on ground |
| `isCrouching` | Bool | Whether character is crouching |
| `isSprinting` | Bool | Whether character is sprinting |
| `isAiming` | Bool | Whether character is aiming |
| `Shoot` | Trigger | Trigger shooting animation |

#### Public Methods:

```csharp
// Trigger shooting animation
void TriggerShootAnimation()

// Get current animation state as string (for debugging)
string GetCurrentState()
```

---

### 4. CharacterAiming

**Script:** `Assets/Scripts/Sebastjan_TL_2/Player/CharacterAiming.cs`

Handles character rotation to face camera direction and controls weapon aiming rig weights.

#### Features:
- Smooth rotation to face camera yaw direction
- Automatic rig weight transitions for aiming
- Delayed return to weapon pose after shooting

#### Inspector Settings:

| Setting | Default | Description |
|---------|---------|-------------|
| `turnSpeed` | 15.0 | Character rotation speed towards camera |
| `weaponAimingRig` | - | Reference to RigLayer_WeaponAiming |
| `aimTransitionSpeed` | 5.0 | Speed of rig weight transition |
| `returnToPoseDelay` | 1.0 | Delay before returning to pose after shooting |

#### Rig Weight System:
- **Weight 0:** Weapon in relaxed pose position
- **Weight 1:** Weapon in aiming position (when firing)
- Smoothly transitions between states

#### Public Methods:

```csharp
// Call when weapon fires to trigger aiming pose
void OnWeaponFired()
```

---

### 5. CharacterController

Unity's built-in CharacterController component for physics-based movement.

#### Configuration:
- **Height:** Dynamically adjusted between standing (2.0) and crouching (1.0)
- **Radius:** Set based on character model
- **Center:** Adjusted to keep bottom of capsule at ground level
- **Slope Limit:** Configurable for terrain navigation
- **Step Offset:** Allows climbing small obstacles

---

### 6. PlayerInput

Unity's Input System component for handling player input.

#### Input Actions Required:
- `Move` (Vector2) - WASD / Left Stick
- `Jump` (Button) - Space / A Button
- `Fire` (Button) - Left Mouse / RT
- `Reload` (Button) - R / X Button
- `Next` (Button) - Mouse Wheel Up / RB
- `Previous` (Button) - Mouse Wheel Down / LB
- `Sprint` (Button) - Left Shift / Left Stick Click
- `Crouch` (Button) - Left Ctrl / B Button

---

### 7. Animator

Unity's Animator component with PlayerAnimationControler controller.

**Animator Controller:** `Assets/src/Sebsastjan/Sci-FI_Trooper_Man_v.3/PlayerAnimationControler.controller`

#### Animation Layers:
1. **Base Layer** - Locomotion and idle animations
2. **Upper Body** - Shooting and aiming animations (optional)

---

### 8. RigBuilder

Unity Animation Rigging component that manages multiple rig layers.

#### Rig Layers:
1. **RigLayer_BodyAim** - Body aiming constraints
2. **RigLayer_WeaponAiming** - Weapon aiming constraints
3. **RigLayer_WeaponPose** - Weapon pose constraints
4. **RigLayer_HandIK** - Hand IK for weapon holding

---

## Animation System

### Available Animations (50 Total)

The character model includes a complete Rifle 8-Way Locomotion Pack with the following animations:

#### Idle Animations (4)
- `idle` - Standard idle pose
- `idle aiming` - Aiming stance
- `idle crouching` - Crouching idle
- `idle crouching aiming` - Crouching aim stance

#### Movement Animations (24)

**Walking (8 directions):**
- `walk forward`, `walk backward`
- `walk left`, `walk right`
- `walk forward left`, `walk forward right`
- `walk backward left`, `walk backward right`

**Running (8 directions):**
- `run forward`, `run backward`
- `run left`, `run right`
- `run forward left`, `run forward right`
- `run backward left`, `run backward right`

**Sprinting (8 directions):**
- `sprint forward`, `sprint backward`
- `sprint left`, `sprint right`
- `sprint forward left`, `sprint forward right`
- `sprint backward left`, `sprint backward right`

#### Crouching Movement (8)
- `walk crouching forward`, `walk crouching backward`
- `walk crouching left`, `walk crouching right`
- `walk crouching forward left`, `walk crouching forward right`
- `walk crouching backward left`, `walk crouching backward right`

#### Jump Animations (3)
- `jump up` - Ascending phase
- `jump loop` - Air time
- `jump down` - Landing phase

#### Turn Animations (4)
- `turn 90 left`, `turn 90 right`
- `crouching turn 90 left`, `crouching turn 90 right`

#### Death Animations (7)
- `death from the front`
- `death from the back`
- `death from right`
- `death from front headshot`
- `death from back headshot`
- `death crouching headshot front`

### Blend Tree System

The PlayerAnimationController uses blend trees for smooth 8-way locomotion:

- **VelocityX** and **VelocityZ** parameters control directional blending
- Normalized to -1 to 1 range based on sprint speed
- Smooth transitions with configurable damping time

### Animation Speed Modifiers

Different movement states have different animation speeds:
- **Idle/Crouch:** 1.0x (normal speed)
- **Running:** 1.25x (configurable via `runAnimationSpeed`)
- **Sprinting:** 1.0x (configurable via `sprintAnimationSpeed`)

---

## Weapon System

### WeaponInventory

**Script:** `Assets/Scripts/Sebastjan_TL_2/Weapons/WeaponInventory.cs`

Manages multiple weapons and switching between them.

#### Features:
- List-based weapon management
- Cycle through weapons with Next/Previous
- Only one weapon active at a time
- Child weapons automatically detected

#### Inspector Settings:
- `weapons` - List of WeaponBase components (children of WeaponPivot)

#### Public Properties:
- `Current` - Currently active weapon (read-only)

#### Public Methods:

```csharp
void Next()  // Switch to next weapon
void Prev()  // Switch to previous weapon
```

---

### WeaponBase (Abstract)

**Script:** `Assets/Scripts/Sebastjan_TL_2/Weapons/WeaponBase.cs`

Abstract base class for all weapons. Implementations include ProjectileWeapon and LazerWeapon.

#### Inspector Settings:

| Setting | Default | Description |
|---------|---------|-------------|
| `magazineSize` | 12 | Bullets per magazine |
| `fireRate` | 6.0 | Shots per second |
| `damage` | 10 | Damage per shot |

#### Visual References:
- `muzzle` - Bullet spawn point
- `muzzleFlash` - Particle system for muzzle flash
- `hitEffect` - Particle system for hit effects
- `tracerEffect` - Trail renderer for bullet tracers
- `raycastOrigin` - Origin point for raycast visualization
- `raycastDestination` - Destination for raycast

#### Events:
- `OnAmmoChanged(int current, int max)` - Invoked when ammo changes

#### Public Methods:

```csharp
// Attempt to fire weapon (returns true if fired)
bool TryFire(Vector3 origin, Vector3 direction)

// Reload weapon to full magazine
void Reload()
```

#### Protected Virtual Methods:

```csharp
// Must be implemented by subclasses
abstract bool DoFire(Vector3 origin, Vector3 direction)

// Override for custom behavior
virtual void OnFired()      // Called after successful fire
virtual void OnDryFire()    // Called when firing with no ammo
virtual void OnReloaded()   // Called after reload
```

---

### Weapon Types

#### 1. ProjectileWeapon

Fires physical projectile GameObjects with Bullet component.

**Features:**
- Instantiates bullet prefabs
- Configurable bullet prefab and speed
- Physics-based projectile movement

#### 2. LazerWeapon

Fires instant-hit raycast "bullets".

**Features:**
- Instant hit detection via raycasting
- No travel time
- Configurable range and penetration

---

### Weapon Integration

The weapon system integrates with multiple systems:

1. **PlayerController3D** - Handles fire/reload input and triggers weapon methods
2. **PlayerAnimationController** - Triggers shooting animations via `TriggerShootAnimation()`
3. **CharacterAiming** - Controls aiming rig weights via `OnWeaponFired()`
4. **AmmoHUD** - Subscribes to `OnAmmoChanged` event for UI updates
5. **SoundManager** - Plays firing and reload sound effects

---

## Movement & Controls

### Movement System

The player uses a **camera-relative movement system**:

1. Input is read as 2D vector (WASD or left stick)
2. Camera's forward and right vectors are projected to horizontal plane
3. Movement direction calculated relative to camera
4. Player rotates to face movement direction

### Movement States

| State | Speed | Trigger | Animation |
|-------|-------|---------|-----------|
| Idle | 0 | No input | Idle / Idle Aiming |
| Walk | 4.0 | WASD (default) | Walk 8-way blend tree |
| Sprint | 9.0 | Hold Shift + WASD | Sprint 8-way blend tree |
| Crouch | 2.0 | Hold Ctrl | Crouch walk blend tree |
| Jump | 5.5 (vertical) | Press Space | Jump up → loop → down |

### Physics

- **CharacterController** provides collision and slope handling
- **Gravity:** -9.81 m/s² (applied continuously)
- **Grounded Detection:** Uses CharacterController.isGrounded
- **Smooth Crouching:** Height lerps at configurable speed

### Input Mapping

Default keyboard/mouse controls:

| Action | Input | Description |
|--------|-------|-------------|
| Move | WASD | Directional movement |
| Sprint | Left Shift | Increase movement speed |
| Crouch | Left Ctrl | Reduce height and speed |
| Jump | Space | Jump vertically |
| Fire | Left Mouse | Fire current weapon |
| Reload | R | Reload current weapon |
| Next Weapon | Mouse Wheel Up | Switch to next weapon |
| Prev Weapon | Mouse Wheel Down | Switch to previous weapon |
| Look | Mouse Movement | Camera rotation (handled by camera system) |

---

## Health System

### Damage Calculation

When damage is applied:

```csharp
float defense = playerController.currentDefense;
float damageTaken = amount * (1.0f - defense);
Current = Max(0, Current - damageTaken);
```

Defense is clamped between 0.0 and 1.0 (0% to 100% reduction).

### Armor System

- Armor is equipped by type (e.g., "Helmet", "Chestplate", "Boots")
- Only one armor piece per type
- Better armor automatically replaces worse armor of the same type
- Total defense is sum of all equipped armor pieces

### Health Events

1. **OnHealthChanged** - Fired on any health change (damage or healing)
2. **OnDied** - Fired when health reaches zero

### Sound Effects

- **Damage Sound:** Plays via `SoundManager.PlayPlayerDamage()`
- **Death Sound:** Plays via `SoundManager.PlayPlayerDeath()`

---

## Visual Effects

### Particle Systems

#### 1. HitEffect (Parent)

Main hit effect container with multiple sub-systems.

**Children:**
- **Sparks** - Bright spark particles on bullet impact
- **Decal** - Impact decal mark
- **Dust** - Dust cloud particles

**Trigger:** Emitted when bullet raycast hits a surface

#### 2. Muzzle Flash (Weapon)

Configured per weapon on WeaponBase component.

**Settings:**
- Single particle burst per shot
- Positioned at weapon muzzle transform
- Uses ParticleSystem.Emit(1)

#### 3. Tracer Effect (Weapon)

Trail renderer that visualizes bullet path.

**Features:**
- Instantiated at raycast origin
- Extends to raycast hit point
- Auto-destroys after lifetime

---

### Animation Rigging Effects

The character uses Unity's Animation Rigging package for procedural animations:

#### RigLayer_BodyAim
- Controls spine and head rotation to look at targets
- Weight controlled by animation controller

#### RigLayer_WeaponAiming
- Aims weapon towards camera direction
- Weight transitions between pose (0) and aim (1)
- Controlled by CharacterAiming script

#### RigLayer_WeaponPose
- Positions weapon in idle/relaxed pose
- Provides right hand IK target

#### RigLayer_HandIK
- Controls left hand placement on weapon
- Ensures proper two-handed grip

---

## Configuration Guide

### Setting Up the Prefab in a Scene

1. **Drag prefab into scene** from `Assets/src/Sebsastjan/Sci-FI_Trooper_Man_v.3/Prefabs/`
2. **Assign Camera Reference:**
   - On PlayerController3D component, assign `cam` to main camera or Cinemachine vcam Follow target
3. **Configure Input:**
   - Ensure Input Actions asset has all required actions (Move, Jump, Fire, etc.)
   - Assign Input Actions asset to PlayerInput component
4. **Setup HUD:**
   - Assign HealthHUD component to Health.healthHUD
   - Weapon system automatically updates AmmoHUD via events
5. **Configure Weapons:**
   - Add weapons as children of WeaponPivot
   - Add WeaponBase components to weapon GameObjects
   - Add weapons to WeaponInventory.weapons list in Inspector
6. **Layer Configuration:**
   - Ensure player is on Layer 3 (Player)
   - Configure layer collision matrix appropriately

---

### Customizing Movement

To adjust movement feel:

```csharp
// In PlayerController3D Inspector:
runSpeed = 5f;        // Faster base movement
sprintSpeed = 12f;    // Faster sprint
jumpSpeed = 7f;       // Higher jumps
gravity = -15f;       // Faster falling
crouchSpeed = 3f;     // Faster crouching
```

---

### Customizing Animation Speed

To adjust animation playback speed:

```csharp
// In PlayerAnimationController Inspector:
runAnimationSpeed = 1.5f;     // Faster run animations
sprintAnimationSpeed = 1.2f;  // Faster sprint animations
velocityDampTime = 0.2f;      // Slower transitions
```

---

### Adding New Weapons

1. Create weapon GameObject as child of WeaponPivot
2. Add ProjectileWeapon or LazerWeapon component
3. Configure weapon stats (damage, fire rate, magazine size)
4. Assign visual references (muzzle, muzzleFlash, hitEffect, tracerEffect)
5. Add weapon to WeaponInventory.weapons list
6. Weapon will automatically integrate with input, HUD, and animation systems

---

### Configuring Armor and Items

Items interact with the player through public methods:

```csharp
// Speed buff
playerController.ApplySpeed(2f, 10);  // +2 speed for 10 seconds

// Jump buff
playerController.ApplyJumpBoost(3f, 0);  // +3 jump permanently

// Equip armor
Armor helmet = new Armor("Iron Helmet", "Helmet", 0.2f);
playerController.EquipArmor(helmet, out Armor replaced);

// Damage buff
PlayerController3D.damageBonus += 5;  // +5 damage (static)

// Healing
health.Heal(25);  // Heal 25 HP

// Max health increase
health.IncreaseMaxHealth(50);  // +50 max HP
```

---

## Usage Instructions

### In-Editor Setup

1. Open `Assets/src/Sebsastjan/Sci-FI_Trooper_Man_v.3/Showcase.unity` for reference setup
2. Prefab is ready to use out-of-the-box in most scenes
3. Requires:
   - Main Camera (or Cinemachine virtual camera)
   - Input System configured
   - UI Canvas with HealthHUD and AmmoHUD components

### Runtime Behavior

**On Awake:**
- Initializes CharacterController and PlayerInput
- Caches input actions
- Sets initial crouch state
- Locks and hides cursor
- Finds Health component

**On Update:**
- Processes input (move, jump, fire, reload, switch weapon)
- Updates movement state (crouch, sprint)
- Applies gravity and movement
- Rotates player to face movement direction
- Updates animation parameters

**On LateUpdate (CharacterAiming):**
- Rotates character to face camera yaw
- Smoothly transitions rig weights

---

### Scripting API Examples

#### Getting Player Reference

```csharp
PlayerController3D player = FindObjectOfType<PlayerController3D>();
```

#### Applying Damage

```csharp
Health health = player.GetComponent<Health>();
health.ApplyDamage(25f);  // Deal 25 damage (reduced by armor)
```

#### Subscribing to Events

```csharp
// Health events
health.OnHealthChanged += (current, max) => {
    Debug.Log($"Health: {current}/{max}");
};

health.OnDied += () => {
    Debug.Log("Player died!");
    // Handle respawn or game over
};

// Weapon events
WeaponBase weapon = player.inventory.Current;
weapon.OnAmmoChanged += (current, max) => {
    Debug.Log($"Ammo: {current}/{max}");
};
```

#### Forcing Weapon Switch

```csharp
player.inventory.Next();  // Switch to next weapon
player.inventory.Prev();  // Switch to previous weapon
```

#### Checking Movement State

```csharp
bool isCrouching = player.IsCrouching;
bool isSprinting = player.IsSprinting;
Vector3 velocity = player.CurrentVelocity;
```

---

## Technical Details

### Performance Considerations

- **Animator Parameters:** Uses hashed parameter IDs for performance
- **Component Caching:** References cached in Awake/Start
- **Event-Driven UI:** HUD updates via events instead of polling
- **Optimized Raycasts:** Only performed when firing weapons

### Dependencies

**Unity Packages:**
- Unity Input System (com.unity.inputsystem)
- Unity Animation Rigging (com.unity.animation.rigging)
- Unity Cinemachine (com.unity.cinemachine) - Optional but recommended

**Custom Scripts:**
- PlayerController3D
- Health
- PlayerAnimationController
- CharacterAiming
- WeaponInventory
- WeaponBase (and subclasses)
- AmmoHUD
- HealthHUD
- SoundManager
- Item system scripts (Armor, Consumable, Equipment, etc.)

### File References

**Prefab:** `Assets/src/Sebsastjan/Sci-FI_Trooper_Man_v.3/Prefabs/SK_SciFiTrooperManV3.prefab`

**Model:** `Assets/src/Sebsastjan/Sci-FI_Trooper_Man_v.3/Meshes/SK_SciFiTrooperManV3.fbx`

**Materials:** `Assets/src/Sebsastjan/Sci-FI_Trooper_Man_v.3/Materials/`

**Textures:** `Assets/src/Sebsastjan/Sci-FI_Trooper_Man_v.3/Textures/`

**Animations:** `Assets/src/Sebsastjan/Rifle 8-Way Locomotion Pack/`

**Animator Controller:** `Assets/src/Sebsastjan/Sci-FI_Trooper_Man_v.3/PlayerAnimationControler.controller`

**Scripts:** `Assets/Scripts/Sebastjan_TL_2/`

---

### Known Limitations

1. **Animation Controller Setup:** Animator controller must be properly configured with all parameters
2. **Input System:** Requires Unity Input System package (old input system not supported)
3. **Camera Dependency:** Movement requires valid camera reference for camera-relative controls
4. **Layer Requirements:** Must be on Player layer (Layer 3) for proper collision handling
5. **Rig Dependencies:** Animation rigging requires proper rig layer setup

---

### Debugging Tips

#### Player Not Moving
- Check that `cam` reference is assigned in PlayerController3D
- Verify Input Actions are configured and assigned to PlayerInput
- Check CharacterController is not blocked by colliders
- Verify player GameObject is active and enabled

#### Animations Not Playing
- Check Animator component has PlayerAnimationControler assigned
- Verify all animator parameters exist in animator controller
- Check animation clips are properly imported and assigned in blend trees
- Enable "Show Debug Info" in PlayerAnimationController Inspector (future feature)

#### Weapons Not Firing
- Verify WeaponInventory.weapons list contains weapon references
- Check weapon has ammo (ammo > 0)
- Verify fire rate cooldown has elapsed
- Check muzzle transform is assigned on weapon

#### Health Not Updating
- Verify HealthHUD is assigned to Health.healthHUD
- Check OnHealthChanged event has subscribers
- Verify ApplyDamage is being called with valid amount

---

## Version History

**Version 1.0** (Current)
- Initial implementation with full animation system
- 8-way locomotion blend trees
- Weapon system integration
- Health and armor system
- Item buff support
- Animation rigging for aiming

---

## Credits

**Team Lead:** Sebastjan (Sebastjan_TL_2)

**Contributors:**
- Player systems and weapons
- Animation controller implementation
- Health system integration
- Weapon inventory system

**Assets:**
- Sci-Fi Trooper Man v3 model
- Rifle 8-Way Locomotion Pack (50 animations)

---

## Support

For issues or questions about this prefab, contact the development team or refer to the main project documentation:

**Project Documentation:** `CLAUDE.md`

**Test Documentation:** `Assets/tst/Sebastjan/TEST_PLAN.md`

**Related Scripts:**
- `Assets/Scripts/Sebastjan_TL_2/Player/PlayerController3D.cs`
- `Assets/Scripts/Sebastjan_TL_2/Player/Health.cs`
- `Assets/Scripts/Sebastjan_TL_2/Player/PlayerAnimationController.cs`
- `Assets/Scripts/Sebastjan_TL_2/Player/CharacterAiming.cs`
- `Assets/Scripts/Sebastjan_TL_2/Weapons/WeaponInventory.cs`
- `Assets/Scripts/Sebastjan_TL_2/Weapons/WeaponBase.cs`

---

*Last Updated: November 16, 2025*