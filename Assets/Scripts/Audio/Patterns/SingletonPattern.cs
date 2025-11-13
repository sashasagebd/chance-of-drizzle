using UnityEngine;

namespace Audio.Patterns
{
    /// <summary>
    /// SINGLETON PATTERN
    /// 
    /// Purpose: Ensure a class has only one instance and provide global access to it.
    /// 
    /// Benefits:
    /// - Single point of access for audio management
    /// - Prevents multiple instances (saves memory, prevents conflicts)
    /// - Global access without passing references
    /// - Controlled initialization
    /// 
    /// Implementation in SoundManager:
    /// - Lazy initialization (creates instance when first accessed)
    /// - Thread-safe (Unity runs on main thread)
    /// - Auto-creation if instance doesn't exist
    /// - DontDestroyOnLoad for persistence
    /// </summary>
    
    /// <summary>
    /// Singleton pattern demonstration
    /// This is a simplified version showing the pattern structure
    /// </summary>
    public class AudioSingleton
    {
        // Private static instance - only one exists
        private static AudioSingleton _instance;
        
        // Private constructor - prevents external instantiation
        private AudioSingleton()
        {
            Debug.Log("AudioSingleton instance created");
        }
        
        // Public static property - global access point
        public static AudioSingleton Instance
        {
            get
            {
                // Lazy initialization - create only when needed
                if (_instance == null)
                {
                    _instance = new AudioSingleton();
                }
                return _instance;
            }
        }
        
        // Example method
        public void PlaySound(string soundName)
        {
            Debug.Log($"Playing sound: {soundName}");
        }
    }
    
    /// <summary>
    /// Unity-specific Singleton pattern (MonoBehaviour version)
    /// This is the pattern used in SoundManager
    /// </summary>
    public class UnityAudioSingleton : MonoBehaviour
    {
        // Private static instance
        private static UnityAudioSingleton _instance;
        
        // Public static property with lazy initialization
        public static UnityAudioSingleton Instance
        {
            get
            {
                // Check if instance exists
                if (_instance == null)
                {
                    // Try to find existing instance in scene
                    _instance = FindFirstObjectByType<UnityAudioSingleton>();
                    
                    // If not found, create new one
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UnityAudioSingleton");
                        _instance = go.AddComponent<UnityAudioSingleton>();
                    }
                }
                return _instance;
            }
        }
        
        void Awake()
        {
            // Ensure only one instance exists
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        
        // Example method
        public void PlayAudio(string audioName)
        {
            Debug.Log($"Playing audio: {audioName}");
        }
    }
    
    /// <summary>
    /// Example usage of Singleton pattern
    /// </summary>
    public class SingletonExample : MonoBehaviour
    {
        void Start()
        {
            DemonstrateSingleton();
        }
        
        void DemonstrateSingleton()
        {
            // Access singleton - same instance every time
            AudioSingleton instance1 = AudioSingleton.Instance;
            AudioSingleton instance2 = AudioSingleton.Instance;
            
            // Verify it's the same instance
            Debug.Log($"Same instance: {ReferenceEquals(instance1, instance2)}");
            
            // Use singleton
            instance1.PlaySound("TestSound");
            
            // Unity singleton example
            UnityAudioSingleton unityInstance = UnityAudioSingleton.Instance;
            unityInstance.PlayAudio("TestAudio");
        }
    }
    
    /// <summary>
    /// Singleton pattern benefits demonstration
    /// </summary>
    public class SingletonBenefits
    {
        // Without Singleton - need to pass reference everywhere
        public void MethodWithoutSingleton(AudioSingleton audio)
        {
            audio.PlaySound("Sound1");
        }
        
        // With Singleton - direct access, no passing needed
        public void MethodWithSingleton()
        {
            AudioSingleton.Instance.PlaySound("Sound1");
        }
        
        // Multiple classes can access same instance
        public void ClassA()
        {
            AudioSingleton.Instance.PlaySound("From Class A");
        }
        
        public void ClassB()
        {
            AudioSingleton.Instance.PlaySound("From Class B"); // Same instance!
        }
    }
}

