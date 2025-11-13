using UnityEngine;

namespace Audio.Patterns
{
    /// <summary>
    /// Demonstrates Static and Dynamic Binding in C#
    /// 
    /// STATIC BINDING (Early Binding):
    /// - Resolved at compile time
    /// - Method calls are determined by the variable's declared type
    /// - Faster execution, no runtime overhead
    /// 
    /// DYNAMIC BINDING (Late Binding):
    /// - Resolved at runtime
    /// - Method calls are determined by the actual object type
    /// - Enables polymorphism and virtual method overriding
    /// </summary>
    
    // Base class for dynamic binding demonstration
    public abstract class AudioHandler
    {
        // STATIC BINDING: This method is bound at compile time
        public void PlayStatic()
        {
            Debug.Log("AudioHandler.PlayStatic() - Static binding");
        }
        
        // DYNAMIC BINDING: Virtual method - resolved at runtime
        public virtual void PlayDynamic()
        {
            Debug.Log("AudioHandler.PlayDynamic() - Base implementation");
        }
        
        // DYNAMIC BINDING: Abstract method - must be overridden
        public abstract void PlayAbstract();
    }
    
    // Derived class demonstrating dynamic binding
    public class WeaponAudioHandler : AudioHandler
    {
        // STATIC BINDING: New method (not virtual) - hides base method
        public new void PlayStatic()
        {
            Debug.Log("WeaponAudioHandler.PlayStatic() - Static binding (hides base)");
        }
        
        // DYNAMIC BINDING: Override virtual method - runtime resolution
        public override void PlayDynamic()
        {
            Debug.Log("WeaponAudioHandler.PlayDynamic() - Dynamic binding (override)");
        }
        
        // DYNAMIC BINDING: Must override abstract method
        public override void PlayAbstract()
        {
            Debug.Log("WeaponAudioHandler.PlayAbstract() - Dynamic binding (abstract override)");
        }
    }
    
    // Another derived class
    public class EnemyAudioHandler : AudioHandler
    {
        public override void PlayDynamic()
        {
            Debug.Log("EnemyAudioHandler.PlayDynamic() - Dynamic binding (override)");
        }
        
        public override void PlayAbstract()
        {
            Debug.Log("EnemyAudioHandler.PlayAbstract() - Dynamic binding (abstract override)");
        }
    }
    
    /// <summary>
    /// Example usage demonstrating static vs dynamic binding
    /// </summary>
    public class BindingExample : MonoBehaviour
    {
        void Start()
        {
            DemonstrateBinding();
        }
        
        void DemonstrateBinding()
        {
            // STATIC BINDING EXAMPLE
            Debug.Log("=== STATIC BINDING (Compile-time) ===");
            
            WeaponAudioHandler weaponHandler = new WeaponAudioHandler();
            weaponHandler.PlayStatic(); // Calls WeaponAudioHandler.PlayStatic()
            
            AudioHandler baseHandler = new WeaponAudioHandler();
            baseHandler.PlayStatic(); // Calls AudioHandler.PlayStatic() - STATIC BINDING
            
            // DYNAMIC BINDING EXAMPLE
            Debug.Log("=== DYNAMIC BINDING (Runtime) ===");
            
            AudioHandler handler1 = new WeaponAudioHandler();
            handler1.PlayDynamic(); // Calls WeaponAudioHandler.PlayDynamic() - DYNAMIC BINDING
            handler1.PlayAbstract(); // Calls WeaponAudioHandler.PlayAbstract() - DYNAMIC BINDING
            
            AudioHandler handler2 = new EnemyAudioHandler();
            handler2.PlayDynamic(); // Calls EnemyAudioHandler.PlayDynamic() - DYNAMIC BINDING
            handler2.PlayAbstract(); // Calls EnemyAudioHandler.PlayAbstract() - DYNAMIC BINDING
            
            // POLYMORPHISM EXAMPLE (Dynamic Binding)
            Debug.Log("=== POLYMORPHISM (Dynamic Binding) ===");
            
            AudioHandler[] handlers = new AudioHandler[]
            {
                new WeaponAudioHandler(),
                new EnemyAudioHandler()
            };
            
            foreach (AudioHandler handler in handlers)
            {
                // Dynamic binding - calls appropriate override at runtime
                handler.PlayDynamic();
                handler.PlayAbstract();
            }
        }
    }
    
    /// <summary>
    /// Interface example for dynamic binding
    /// </summary>
    public interface IPlayable
    {
        void Play(); // Interface methods are always dynamically bound
    }
    
    public class WeaponSound : IPlayable
    {
        public void Play()
        {
            Debug.Log("WeaponSound.Play() - Interface implementation (dynamic binding)");
        }
    }
    
    public class EnemySound : IPlayable
    {
        public void Play()
        {
            Debug.Log("EnemySound.Play() - Interface implementation (dynamic binding)");
        }
    }
    
    /// <summary>
    /// Interface-based dynamic binding example
    /// </summary>
    public class InterfaceBindingExample : MonoBehaviour
    {
        void Start()
        {
            IPlayable[] sounds = new IPlayable[]
            {
                new WeaponSound(),
                new EnemySound()
            };
            
            foreach (IPlayable sound in sounds)
            {
                sound.Play(); // Dynamic binding - calls appropriate implementation
            }
        }
    }
}

