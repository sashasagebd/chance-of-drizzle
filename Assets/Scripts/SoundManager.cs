using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
	[Header("Mixer (optional)")]
	[SerializeField] private AudioMixer audioMixer;
	[SerializeField] private AudioMixerGroup sfxMixerGroup;
	[SerializeField] private AudioMixerGroup musicMixerGroup;

	[Header("Default Clips (assign as needed)")]
	public AudioClip sfxWeaponFire;
	public AudioClip sfxWeaponReload;
	public AudioClip sfxWeaponDryFire;
	public AudioClip sfxHit;
	public AudioClip sfxUIClick;
	public AudioClip musicStageLoop;

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

	readonly Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
	AudioSource _musicSource;
	AudioSource[] _oneShotSources;
	int _oneShotIndex;

	void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
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
		_musicSource.outputAudioMixerGroup = musicMixerGroup;

		// SFX pooled 3D sources
		for (int i = 0; i < sfxPoolSize; i++)
		{
			var src = new GameObject($"SFX_Source_{i}").AddComponent<AudioSource>();
			src.transform.SetParent(transform);
			src.spatialBlend = 1f; // 3D by default
			src.playOnAwake = false;
			src.outputAudioMixerGroup = sfxMixerGroup;
			_sfxPool.Enqueue(src);
		}

		// 2D one-shot voices (UI, small SFX)
		_oneShotSources = new AudioSource[Mathf.Max(1, oneShotVoices)];
		for (int i = 0; i < _oneShotSources.Length; i++)
		{
			_oneShotSources[i] = gameObject.AddComponent<AudioSource>();
			_oneShotSources[i].spatialBlend = 0f; // 2D
			_oneShotSources[i].playOnAwake = false;
			_oneShotSources[i].outputAudioMixerGroup = sfxMixerGroup;
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
		temp.transform.SetParent(transform);
		temp.spatialBlend = 1f;
		temp.playOnAwake = false;
		temp.outputAudioMixerGroup = sfxMixerGroup;
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
		StopAllCoroutines();
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

	IEnumerator ReturnWhenDone(AudioSource src)
	{
		while (src != null && src.isPlaying)
			yield return null;
		ReturnSfxSource(src);
	}

	// Convenience wrappers for default clips (assign in Inspector)
	public void PlayWeaponFire(Vector3 position, float volume = 1f) => PlaySfxAt(sfxWeaponFire, position, volume);
	public void PlayWeaponReload(Vector3 position, float volume = 1f) => PlaySfxAt(sfxWeaponReload, position, volume);
	public void PlayWeaponDryFire(Vector3 position, float volume = 1f) => PlaySfxAt(sfxWeaponDryFire, position, volume);
	public void PlayHitAt(Vector3 position, float volume = 1f) => PlaySfxAt(sfxHit, position, volume);
	public void PlayUIClick(float volume = 1f) => PlaySfx2D(sfxUIClick, volume);
	public void StartStageMusic(float volume = 1f) => PlayMusic(musicStageLoop, true, volume);
}
