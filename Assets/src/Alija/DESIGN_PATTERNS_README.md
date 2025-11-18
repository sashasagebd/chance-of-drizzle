 Design Patterns in Audio System

This document describes the design patterns implemented in the audio system.

Patterns Implemented

1. Singleton Pattern
2. Location: `SoundManager.cs` (lines 51-67)

Purpose: Ensure only one instance of SoundManager exists and provide global access.

Implementation:
```csharp
static SoundManager _instance;
public static SoundManager Instance
{
    get
    {
        if (_instance == null)
        {
            _instance = FindFirstObjectByType<SoundManager>();
            if (_instance == null)
            {
                var go = new GameObject("SoundManager");
                _instance = go.AddComponent<SoundManager>();
            }
        }
        return _instance;
    }
}
```

Benefits:
- Single point of access for all audio operations
- Prevents multiple instances (saves memory, prevents conflicts)
- Global access without passing references
- Lazy initialization (creates when needed)

Usage:
```csharp
SoundManager.Instance.PlayWeaponFire(position);
```

`Patterns/SingletonPattern.cs` for detailed explanation and examples.

---
2. Private Class Data Pattern 
Location: `Patterns/PrivateClassDataPattern.cs`

Purpose: Restrict access to internal data by encapsulating it in a private data class.

Implementation:
```csharp
public class AudioSourceData
{
    private readonly float _volume;
    private readonly float _pitch;
    // ... other private fields
    
    // Read-only properties
    public float Volume => _volume;
    
    // No setters - data is immutable
}
```

Benefits:
- Prevents unauthorized access to internal data
- Reduces coupling between classes
- Makes data immutable or controlled access
- Improves maintainability

Usage:
```csharp
// Create protected data
AudioSourceData data = AudioSourceDataFactory.Create3D(0.8f, 1.2f);

// Use in protected class
ProtectedAudioSource source = new ProtectedAudioSource(audioSource, data);

// Access through controlled interface
float volume = source.GetVolume(); // OK - read access
// source.SetVolume(0.5f); // NOT POSSIBLE - data is protected
```
see : `Patterns/PrivateClassDataPattern.cs` for full implementation.

---

## Additional Patterns Demonstrated

### 3. Static and Dynamic Binding
**Location**: `Patterns/StaticDynamicBinding.cs`

**Static Binding (Early Binding)**:
- Resolved at compile time
- Method calls determined by variable's declared type
- Faster execution, no runtime overhead

**Dynamic Binding (Late Binding)**:
- Resolved at runtime
- Method calls determined by actual object type
- Enables polymorphism and virtual method overriding

**Example**:
```csharp
// Static binding
AudioHandler handler = new WeaponAudioHandler();
handler.PlayStatic(); // Calls AudioHandler.PlayStatic() - STATIC

// Dynamic binding
handler.PlayDynamic(); // Calls WeaponAudioHandler.PlayDynamic() - DYNAMIC
```

See: `Patterns/StaticDynamicBinding.cs` for comprehensive examples.

---

## Pattern Relationships

### Singleton + Private Class Data
- Singleton provides global access
- Private Class Data protects internal configuration
- Together: Global access with controlled data modification

### Dynamic Binding + Polymorphism
- Virtual methods enable runtime method resolution
- Interfaces provide contract-based polymorphism
- Abstract classes define base behavior

---

## Pattern Selection Rationale

### Why Singleton?
- Audio System Requirement: Only one audio manager needed
- Global Access: Many systems need audio (weapons, enemies, UI, etc.)
- Resource Management: Single point for audio source pooling
- State Management: Centralized audio state (volume, mute, etc.)

### Why Private Class Data?
- Data Protection: Audio configuration should not be modified arbitrarily
- Immutability: Audio settings should be consistent once set
- Validation: Ensures valid audio parameters (volume 0-1, pitch limits)
- Maintainability: Changes to audio data structure don't break other code

### Why Demonstrate Static/Dynamic Binding?
- Understanding Polymorphism: Shows how C# handles method resolution
- Performance: Demonstrates compile-time vs runtime resolution
- Flexibility: Shows when to use virtual methods vs static methods

---

## Pattern Usage in Audio System

### SoundManager (Singleton)
- Global access: `SoundManager.Instance.PlayWeaponFire()`
- Single instance: Prevents multiple audio managers
- Persistence: DontDestroyOnLoad across scenes

### AudioSourceData (Private Class Data)
- Protected configuration: Volume, pitch, distance settings
- Factory methods: `AudioSourceDataFactory.Create3D()`
- Immutability: Cannot modify after creation

### Component Classes (Dynamic Binding)
- HealthAudio, EnemyAudio, WeaponAudio use polymorphism
- Virtual methods in WeaponBase enable overrides
- Interface-based design for extensibility

---

## Testing Patterns

# Singleton Tests
- Test single instance creation
- Test instance persistence
- Test duplicate prevention

# Private Class Data Tests
- Test data immutability
- Test controlled access
- Test factory methods

# Binding Tests
- Test static vs dynamic method resolution
- Test polymorphism behavior
- Test interface implementations

---

## Best Practices

1. Singleton: Use sparingly, only for true singletons (like managers)
2. Private Class Data: Use for configuration objects that should be immutable
3. Dynamic Binding: Use virtual/abstract methods when behavior varies by type
4. Static Binding: Use for performance-critical code where type is known

---

## References

- Singleton Pattern: Gang of Four Design Patterns
- Private Class Data: Encapsulation pattern for data protection
- Static/Dynamic Binding: C# language feature documentation

