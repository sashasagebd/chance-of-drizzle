using UnityEngine;

namespace Audio.Patterns
{
    /// <summary>
    /// PRIVATE CLASS DATA PATTERN
    /// 
    /// Purpose: Restrict access to internal data by encapsulating it in a private data class.
    /// Benefits:
    /// - Prevents unauthorized access to internal data
    /// - Reduces coupling between classes
    /// - Makes data immutable or controlled access
    /// - Improves maintainability
    /// 
    /// This pattern is used in AudioSourceData to protect audio configuration.
    /// </summary>
    
    /// <summary>
    /// Private data class - encapsulates audio source configuration
    /// This data is private and can only be accessed through controlled methods
    /// </summary>
    public class AudioSourceData
    {
        // Private fields - cannot be accessed directly
        private readonly float _volume;
        private readonly float _pitch;
        private readonly float _minDistance;
        private readonly float _maxDistance;
        private readonly AudioRolloffMode _rolloffMode;
        private readonly bool _is3D;
        
        // Constructor - only way to set data
        public AudioSourceData(float volume, float pitch, float minDistance, float maxDistance, 
                              AudioRolloffMode rolloffMode, bool is3D)
        {
            _volume = Mathf.Clamp01(volume);
            _pitch = Mathf.Clamp(pitch, -3f, 3f);
            _minDistance = minDistance;
            _maxDistance = maxDistance;
            _rolloffMode = rolloffMode;
            _is3D = is3D;
        }
        
        // Read-only properties - controlled access
        public float Volume => _volume;
        public float Pitch => _pitch;
        public float MinDistance => _minDistance;
        public float MaxDistance => _maxDistance;
        public AudioRolloffMode RolloffMode => _rolloffMode;
        public bool Is3D => _is3D;
        
        // No setters - data is immutable after construction
    }
    
    /// <summary>
    /// Class that uses Private Class Data pattern
    /// Audio configuration is protected and can only be accessed through controlled interface
    /// </summary>
    public class ProtectedAudioSource
    {
        // Private data object - encapsulates all configuration
        private readonly AudioSourceData _data;
        private AudioSource _audioSource;
        
        // Constructor - creates private data
        public ProtectedAudioSource(AudioSource audioSource, AudioSourceData data)
        {
            _audioSource = audioSource;
            _data = data;
            ApplyConfiguration();
        }
        
        // Apply configuration from private data
        private void ApplyConfiguration()
        {
            if (_audioSource == null) return;
            
            _audioSource.volume = _data.Volume;
            _audioSource.pitch = _data.Pitch;
            _audioSource.spatialBlend = _data.Is3D ? 1f : 0f;
            _audioSource.minDistance = _data.MinDistance;
            _audioSource.maxDistance = _data.MaxDistance;
            _audioSource.rolloffMode = _data.RolloffMode;
        }
        
        // Public method - controlled access to data
        public float GetVolume()
        {
            return _data.Volume; // Read-only access
        }
        
        // Cannot modify data directly - must create new instance
        public ProtectedAudioSource WithNewVolume(float newVolume)
        {
            var newData = new AudioSourceData(
                newVolume,
                _data.Pitch,
                _data.MinDistance,
                _data.MaxDistance,
                _data.RolloffMode,
                _data.Is3D
            );
            return new ProtectedAudioSource(_audioSource, newData);
        }
    }
    
    /// <summary>
    /// Factory for creating AudioSourceData with sensible defaults
    /// Uses Private Class Data pattern to ensure valid configurations
    /// </summary>
    public static class AudioSourceDataFactory
    {
        // Factory methods - ensure valid data creation
        public static AudioSourceData Create3D(float volume = 1f, float pitch = 1f)
        {
            return new AudioSourceData(volume, pitch, 2f, 50f, AudioRolloffMode.Logarithmic, true);
        }
        
        public static AudioSourceData Create2D(float volume = 1f, float pitch = 1f)
        {
            return new AudioSourceData(volume, pitch, 0f, 0f, AudioRolloffMode.Logarithmic, false);
        }
        
        public static AudioSourceData CreateWeapon(float volume = 1f)
        {
            return new AudioSourceData(volume, 1f, 2f, 30f, AudioRolloffMode.Logarithmic, true);
        }
        
        public static AudioSourceData CreateEnemy(float volume = 1f)
        {
            return new AudioSourceData(volume, 1f, 3f, 50f, AudioRolloffMode.Logarithmic, true);
        }
    }
    
    /// <summary>
    /// Example usage of Private Class Data pattern
    /// </summary>
    public class PrivateClassDataExample : MonoBehaviour
    {
        void Start()
        {
            DemonstratePrivateClassData();
        }
        
        void DemonstratePrivateClassData()
        {
            // Create AudioSource
            GameObject go = new GameObject("TestAudio");
            AudioSource audioSource = go.AddComponent<AudioSource>();
            
            // Create private data using factory
            AudioSourceData data = AudioSourceDataFactory.Create3D(0.8f, 1.2f);
            
            // Create protected audio source
            ProtectedAudioSource protectedSource = new ProtectedAudioSource(audioSource, data);
            
            // Access data through controlled interface
            float volume = protectedSource.GetVolume(); // OK - read access
            Debug.Log($"Volume: {volume}");
            
            // Cannot modify directly - must create new instance
            ProtectedAudioSource newSource = protectedSource.WithNewVolume(0.5f);
            
            // Data is immutable - original unchanged
            Debug.Log($"Original volume: {protectedSource.GetVolume()}");
            Debug.Log($"New volume: {newSource.GetVolume()}");
            
            // Cleanup
            Destroy(go);
        }
    }
    
    /// <summary>
    /// Extended example: Audio Settings using Private Class Data
    /// Multiple related settings are grouped together and protected
    /// </summary>
    public class AudioSettingsData
    {
        private readonly float _masterVolume;
        private readonly float _sfxVolume;
        private readonly float _musicVolume;
        private readonly bool _muteOnFocusLoss;
        
        public AudioSettingsData(float masterVolume, float sfxVolume, float musicVolume, bool muteOnFocusLoss)
        {
            _masterVolume = Mathf.Clamp01(masterVolume);
            _sfxVolume = Mathf.Clamp01(sfxVolume);
            _musicVolume = Mathf.Clamp01(musicVolume);
            _muteOnFocusLoss = muteOnFocusLoss;
        }
        
        // Read-only access
        public float MasterVolume => _masterVolume;
        public float SfxVolume => _sfxVolume;
        public float MusicVolume => _musicVolume;
        public bool MuteOnFocusLoss => _muteOnFocusLoss;
        
        // Immutable - create new instance to change
        public AudioSettingsData WithMasterVolume(float newVolume)
        {
            return new AudioSettingsData(newVolume, _sfxVolume, _musicVolume, _muteOnFocusLoss);
        }
    }
}

