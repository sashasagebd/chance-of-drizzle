using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	[Header("Weapon Clips")]
	public AudioClip sfxWeaponFire;
	public AudioClip sfxWeaponReload;
	public AudioClip sfxWeaponDryFire;
	public AudioClip sfxHit;
	
	[Header("Enemy Clips")]
	public AudioClip sfxEnemyAttack;
	public AudioClip sfxEnemyDeath;
	public AudioClip sfxEnemyHit;
	public AudioClip sfxEnemyLaser;
	public AudioClip sfxEnemyMissile;
	
	[Header("Player Status Clips")]
	public AudioClip sfxPlayerDamage;
	public AudioClip sfxPlayerHeal;
	public AudioClip sfxPlayerDeath;
	public AudioClip sfxPlayerLowHealth;
	public AudioClip sfxLevelUp;
	public AudioClip sfxItemPickup;
	
	[Header("UI Clips")]
	public AudioClip sfxUIClick;
	public AudioClip sfxUISelect;
	
	[Header("Music & Ambient")]
	public AudioClip musicStageLoop;
	public AudioClip musicMenu;
	public AudioClip ambientLoop;
	
	[Header("Map/Level Transition")]
	public AudioClip sfxMapTransition;
	public AudioClip sfxLevelComplete;

	[Header("Pool Settings")]
	[SerializeField] private int sfxPoolSize = 16;
	[SerializeField] private int oneShotVoices = 8; // backup for PlayOneShot

	static SoundManager _instance;
	public static SoundManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<SoundManager>();
				if (_instance == null)
				{
					var go = new GameObject("SoundManager");
					_instance = go.AddComponent<SoundManager>();
				}
			}
			return _instance;
		}
	}

	readonly Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
	AudioSource _musicSource;
	AudioSource _ambientSource;
	AudioSource[] _oneShotSources;
	int _oneShotIndex;
	private Coroutine FadeAmbientInRoutine;
    private Coroutine FadeAmbientOutRoutine;

	void Awake()
	{
		if (_instance != null && _instance != this)
		{
			DestroyImmediate(gameObject);
			return;
		}
		_instance = this;
		DontDestroyOnLoad(gameObject);

		BuildPools();
	}

	void BuildPools()
	{
		// Music
		_musicSource = gameObject.AddComponent<AudioSource>();
		_musicSource.loop = true;
		_musicSource.playOnAwake = false;
		
		// Ambient
		_ambientSource = gameObject.AddComponent<AudioSource>();
		_ambientSource.loop = true;
		_ambientSource.playOnAwake = false;
		_ambientSource.volume = 0.3f; // Ambient typically quieter

		// SFX pooled 3D sources
		for (int i = 0; i < sfxPoolSize; i++)
		{
			var src = new GameObject($"SFX_Source_{i}").AddComponent<AudioSource>();
			src.transform.SetParent(null);
			src.spatialBlend = 1f; // 3D by default
			src.playOnAwake = false;
			_sfxPool.Enqueue(src);
		}

		// 2D one-shot voices (UI, small SFX)
		_oneShotSources = new AudioSource[Mathf.Max(1, oneShotVoices)];
		for (int i = 0; i < _oneShotSources.Length; i++)
		{
			_oneShotSources[i] = gameObject.AddComponent<AudioSource>();
			_oneShotSources[i].spatialBlend = 0f; // 2D
			_oneShotSources[i].playOnAwake = false;
		}
	}

	AudioSource RentSfxSource()
	{
		if (_sfxPool.Count > 0)
		{
			var src = _sfxPool.Dequeue();
			return src;
		}
		// If exhausted, create a temporary one
		var temp = new GameObject("SFX_Source_Temp").AddComponent<AudioSource>();
		temp.transform.SetParent(null);
		temp.spatialBlend = 1f;
		temp.playOnAwake = false;
		return temp;
	}

	void ReturnSfxSource(AudioSource src)
	{
		if (src == null) return;
		if (src.gameObject.name.StartsWith("SFX_Source_") && _sfxPool.Count < sfxPoolSize)
		{
			_sfxPool.Enqueue(src);
		}
		else
		{
			Destroy(src.gameObject);
		}
	}

	public void PlaySfxAt(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
	{
		if (clip == null) return;
		var src = RentSfxSource();
		src.transform.position = position;
		src.clip = clip;
		src.volume = Mathf.Clamp01(volume);
		src.pitch = Mathf.Clamp(pitch, -3f, 3f);
		src.spatialBlend = 1f;
		src.minDistance = 2f;
		src.maxDistance = 50f;
		src.rolloffMode = AudioRolloffMode.Logarithmic;
		src.Play();
		StartCoroutine(ReturnWhenDone(src));
	}

	public void PlaySfx2D(AudioClip clip, float volume = 1f, float pitch = 1f)
	{
		if (clip == null) return;
		var src = _oneShotSources[_oneShotIndex++ % _oneShotSources.Length];
		src.pitch = Mathf.Clamp(pitch, -3f, 3f);
		src.volume = Mathf.Clamp01(volume);
		src.PlayOneShot(clip, src.volume);
	}

	public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f, float fadeSeconds = 0f)
	{
		if (_musicSource == null) return;
		if (fadeSeconds > 0f)
		{
			StartCoroutine(FadeMusic(clip, loop, volume, fadeSeconds));
		}
		else
		{
			_musicSource.loop = loop;
			_musicSource.clip = clip;
			_musicSource.volume = Mathf.Clamp01(volume);
			if (clip == null)
			{
				_musicSource.Stop();
				return;
			}
			_musicSource.Play();
		}
	}
	
	public void PlayAmbient(AudioClip clip, float volume = 0.3f, bool fadeIn = true)
    {
        if (_ambientSource == null || clip == null) return;

        // Stop only existing ambient fades
        if (FadeAmbientInRoutine != null) StopCoroutine(FadeAmbientInRoutine);
        if (FadeAmbientOutRoutine != null) StopCoroutine(FadeAmbientOutRoutine);

        // Reset source
        _ambientSource.Stop();
        _ambientSource.clip = clip;
        _ambientSource.volume = Mathf.Clamp01(volume);

        if (fadeIn)
        {
            FadeAmbientInRoutine = StartCoroutine(FadeAmbientIn());
        }
        else
        {
            _ambientSource.Play();
        }
    }
	
	public void StopAmbient(float fadeOutSeconds = 1f)
    {
        if (_ambientSource == null || !_ambientSource.isPlaying) return;

        if (FadeAmbientInRoutine != null)
        {
            StopCoroutine(FadeAmbientInRoutine);
			FadeAmbientInRoutine = null;
        } 
        if (FadeAmbientOutRoutine != null)
        {
            StopCoroutine(FadeAmbientOutRoutine);
			FadeAmbientOutRoutine = null;
        } 

        if (fadeOutSeconds > 0f)
        {
            FadeAmbientOutRoutine = StartCoroutine(FadeAmbientOut(fadeOutSeconds));
        }
        else
        {
            _ambientSource.Stop();
        }
    }
	
	IEnumerator FadeMusic(AudioClip newClip, bool loop, float targetVolume, float duration)
	{
		float startVolume = _musicSource.volume;
		float elapsed = 0f;
		
		// Fade out current
		while (elapsed < duration / 2f && _musicSource.isPlaying)
		{
			elapsed += Time.deltaTime;
			_musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2f));
			yield return null;
		}
		
		// Switch clip
		_musicSource.Stop();
		_musicSource.loop = loop;
		_musicSource.clip = newClip;
		if (newClip != null)
		{
			_musicSource.Play();
		}
		
		// Fade in new
		elapsed = 0f;
		while (elapsed < duration / 2f && _musicSource.isPlaying)
		{
			elapsed += Time.deltaTime;
			_musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (duration / 2f));
			yield return null;
		}
		_musicSource.volume = targetVolume;
	}
	
	IEnumerator FadeAmbientIn()
	{
		_ambientSource.volume = 0f;
		_ambientSource.Play();
		float targetVolume = 0.3f;
		float elapsed = 0f;
		float duration = 2f;
		
		while (elapsed < duration && _ambientSource.isPlaying)
		{
			elapsed += Time.deltaTime;
			_ambientSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
			yield return null;
		}
		_ambientSource.volume = targetVolume;
		FadeAmbientInRoutine = null;
	}
	
	IEnumerator FadeAmbientOut(float duration)
	{
		float startVolume = _ambientSource.volume;
		float elapsed = 0f;
		
		while (elapsed < duration && _ambientSource.isPlaying)
		{
			elapsed += Time.deltaTime;
			_ambientSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
			yield return null;
		}
		_ambientSource.Stop();
		FadeAmbientOutRoutine = null;
	}

	IEnumerator ReturnWhenDone(AudioSource src)
	{
		while (src != null && src.isPlaying)
			yield return null;
		ReturnSfxSource(src);
	}

	// Convenience wrappers for default clips (assign in Inspector)
	
	// Weapon sounds
	public void PlayWeaponFire(Vector3 position, float volume = 1f) => PlaySfxAt(sfxWeaponFire, position, volume);
	public void PlayWeaponReload(Vector3 position, float volume = 1f) => PlaySfxAt(sfxWeaponReload, position, volume);
	public void PlayWeaponDryFire(Vector3 position, float volume = 1f) => PlaySfxAt(sfxWeaponDryFire, position, volume);
	public void PlayHitAt(Vector3 position, float volume = 1f) => PlaySfxAt(sfxHit, position, volume);
	
	// Enemy sounds
	public void PlayEnemyAttack(Vector3 position, float volume = 1f) => PlaySfxAt(sfxEnemyAttack, position, volume);
	public void PlayEnemyDeath(Vector3 position, float volume = 1f) => PlaySfxAt(sfxEnemyDeath, position, volume);
	public void PlayEnemyHit(Vector3 position, float volume = 1f) => PlaySfxAt(sfxEnemyHit, position, volume);
	public void PlayEnemyLaser(Vector3 position, float volume = 1f) => PlaySfxAt(sfxEnemyLaser, position, volume);
	public void PlayEnemyMissile(Vector3 position, float volume = 1f) => PlaySfxAt(sfxEnemyMissile, position, volume);
	
	// Player status sounds
	public void PlayPlayerDamage(float volume = 1f) => PlaySfx2D(sfxPlayerDamage, volume);
	public void PlayPlayerHeal(float volume = 1f) => PlaySfx2D(sfxPlayerHeal, volume);
	public void PlayPlayerDeath(float volume = 1f) => PlaySfx2D(sfxPlayerDeath, volume);
	public void PlayPlayerLowHealth(float volume = 1f) => PlaySfx2D(sfxPlayerLowHealth, volume);
	public void PlayLevelUp(float volume = 1f) => PlaySfx2D(sfxLevelUp, volume);
	public void PlayItemPickup(float volume = 1f) => PlaySfx2D(sfxItemPickup, volume);
	
	// UI sounds
	public void PlayUIClick(float volume = 1f) => PlaySfx2D(sfxUIClick, volume);
	public void PlayUISelect(float volume = 1f) => PlaySfx2D(sfxUISelect, volume);
	
	// Music & ambient
	public void StartStageMusic(float volume = 1f) => PlayMusic(musicStageLoop, true, volume);
	public void PlayMenuMusic(float volume = 1f) => PlayMusic(musicMenu, true, volume);
	public void StartAmbient(float volume = 0.3f) => PlayAmbient(ambientLoop, volume);
	
	// Map/level sounds
	public void PlayMapTransition(float volume = 1f) => PlaySfx2D(sfxMapTransition, volume);
	public void PlayLevelComplete(float volume = 1f) => PlaySfx2D(sfxLevelComplete, volume);



}

