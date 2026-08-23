using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Controlador central de audio del proyecto (Arquitectura Profesional).
/// Soporta Música Dinámica (Stems), Aleatoriedad de Tono (Pitch), y Canales Independientes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static AudioManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Constants — AudioMixer parameters and PlayerPrefs keys
    // -------------------------------------------------------------------------
    private const string MIXER_PARAM_MASTER = "MasterVolume";
    private const string MIXER_PARAM_MUSIC  = "MusicVolume";
    private const string MIXER_PARAM_SFX    = "SFXVolume";

    public const string PREF_KEY_MASTER = "Volume_Master";
    public const string PREF_KEY_MUSIC  = "Volume_Music";
    public const string PREF_KEY_SFX    = "Volume_SFX";

    private const float DEFAULT_VOLUME = 1f;

    // -------------------------------------------------------------------------
    // Serializable Mapping Structs
    // -------------------------------------------------------------------------
    [Serializable]
    public struct SFXEntry
    {
        [Tooltip("ID único (ej: 'UI_Click', 'Fish_Caught').")]
        public string id;
        public AudioClip clip;
        [Tooltip("Si es true, variará levemente el tono (pitch) cada vez que se reproduzca para no fatigar el oído.")]
        public bool randomizePitch;
        [Range(0f, 1f)]
        public float volume;
    }

    [Serializable]
    public struct BGMEntry
    {
        [Tooltip("ID único (ej: 'Music_Menu', 'Music_Gameplay').")]
        public string id;
        public AudioClip clip;
    }

    // -------------------------------------------------------------------------
    // Inspector — Audio Mixer & Channels
    // -------------------------------------------------------------------------
    [Header("Audio Mixer")]
    [Tooltip("El AudioMixer principal del proyecto (donde están los parámetros Master, Music y SFX)")]
    [SerializeField] private AudioMixer _mainMixer;
    
    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup _musicMixerGroup;
    [SerializeField] private AudioMixerGroup _sfxMixerGroup;

    public AudioMixerGroup SFXMixerGroup => _sfxMixerGroup;

    // -------------------------------------------------------------------------
    // Inspector — Sound Libraries
    // -------------------------------------------------------------------------
    [Header("Sound Libraries")]
    [SerializeField] private List<SFXEntry> _sfxLibrary = new List<SFXEntry>();
    [SerializeField] private List<BGMEntry> _bgmLibrary = new List<BGMEntry>();

    [Header("Auto-Play Settings")]
    [Tooltip("ID de la música base que suena al iniciar (Ej: 'Music_Menu')")]
    [SerializeField] private string _mainMenuMusicId;
    [Tooltip("ID del ambiente que suena al iniciar (Ej: 'Amb_River')")]
    [SerializeField] private string _defaultAmbienceId;

    [Header("Transition Settings")]
    [SerializeField][Range(0f, 1f)] private float _musicTargetVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float _ambienceTargetVolume = 0.5f;

    // -------------------------------------------------------------------------
    // Runtime — Audio Sources and Internal State
    // -------------------------------------------------------------------------
    private AudioSource _musicSourceBase;
    private AudioSource _musicSourceTension; // Para el Stem de Tensión
    private AudioSource _sfxSource;
    private AudioSource _ambienceSource;
    
    private Coroutine _fadeCoroutine;
    private Coroutine _tensionFadeCoroutine;

    // O(1) lookup dictionaries
    private Dictionary<string, SFXEntry> _sfxDict;
    private Dictionary<string, AudioClip> _bgmDict;

    // Object Pool para SFX con Pitch Variable (evita pisar el pitch global)
    private Queue<AudioSource> _sfxPool = new Queue<AudioSource>();

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Crear Canales de Audio Dedicados
        _musicSourceBase = CreateAudioSource("MusicSource_Base", _musicMixerGroup, loop: true);
        _musicSourceTension = CreateAudioSource("MusicSource_Tension", _musicMixerGroup, loop: true);
        _sfxSource = CreateAudioSource("SFXSource_Global", _sfxMixerGroup, loop: false);
        _ambienceSource = CreateAudioSource("AmbienceSource", _musicMixerGroup, loop: true);

        _musicSourceBase.volume = 0f;
        _musicSourceTension.volume = 0f;
        _ambienceSource.volume = 0f;

        // Pre-crear pool de SFX
        for(int i = 0; i < 5; i++)
        {
            AudioSource poolSrc = CreateAudioSource($"SFXPool_{i}", _sfxMixerGroup, loop: false);
            _sfxPool.Enqueue(poolSrc);
        }

        BuildLookupDictionaries();
    }

    private void Start()
    {
        LoadSavedVolumePreferences();

        if (!string.IsNullOrEmpty(_mainMenuMusicId))
            PlayBGM(_mainMenuMusicId);
            
        if (!string.IsNullOrEmpty(_defaultAmbienceId))
            PlayAmbience(_defaultAmbienceId);
    }

    private void OnEnable() => GameManager.OnStateChanged += HandleStateChanged;
    private void OnDisable() => GameManager.OnStateChanged -= HandleStateChanged;

    private void HandleStateChanged(GameManager.GameState newState)
    {
        // Puntos de extensión para cambiar música automáticamente según el GameManager
        switch (newState)
        {
            case GameManager.GameState.MainMenu:
                FadeTensionStem(0f, 1f);
                PlayBGM(_mainMenuMusicId);
                break;
            case GameManager.GameState.Playing:
                // Aquí el juego puede llamar a PlayMusicStems() si lo prefiere.
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Public Playback API - MÚSICA Y STEMS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reproduce un track normal (sin stem de tensión).
    /// </summary>
    public void PlayBGM(string bgmId, float fadeDuration = 1f)
    {
        if (!_bgmDict.TryGetValue(bgmId, out AudioClip clip)) return;
        PlayMusicWithCrossfade(clip, null, fadeDuration);
    }

    /// <summary>
    /// Reproduce dos tracks Sincronizados (Base y Tensión). 
    /// El de tensión arranca mudo.
    /// </summary>
    public void PlayMusicStems(string baseId, string tensionId, float fadeDuration = 1f)
    {
        AudioClip baseClip = _bgmDict.ContainsKey(baseId) ? _bgmDict[baseId] : null;
        AudioClip tensionClip = _bgmDict.ContainsKey(tensionId) ? _bgmDict[tensionId] : null;
        
        PlayMusicWithCrossfade(baseClip, tensionClip, fadeDuration);
    }

    /// <summary>
    /// Desvanece el volumen del Stem de Tensión. 
    /// Úsalo cuando queden 5 segundos o 2 jugadores.
    /// </summary>
    public void FadeTensionStem(float targetVolume, float duration = 1f)
    {
        if (_tensionFadeCoroutine != null) StopCoroutine(_tensionFadeCoroutine);
        _tensionFadeCoroutine = StartCoroutine(FadeVolumeRoutine(_musicSourceTension, _musicSourceTension.volume, targetVolume, duration));
    }

    // -------------------------------------------------------------------------
    // Public Playback API - AMBIENTE Y SFX
    // -------------------------------------------------------------------------
    
    public void PlayAmbience(string ambId, float fadeDuration = 2f)
    {
        if (!_bgmDict.TryGetValue(ambId, out AudioClip clip)) return;
        
        _ambienceSource.clip = clip;
        _ambienceSource.Play();
        StartCoroutine(FadeVolumeRoutine(_ambienceSource, 0f, _ambienceTargetVolume, fadeDuration));
    }

    public void StopAmbience(float fadeDuration = 1f)
    {
        StartCoroutine(FadeVolumeRoutine(_ambienceSource, _ambienceSource.volume, 0f, fadeDuration));
    }

    /// <summary>
    /// Reproduce un efecto de sonido buscando por ID. 
    /// Aplica automáticamente el Pitch y Volumen configurado, o uno forzado (pitchOverride).
    /// </summary>
    public void PlaySFX(string sfxId, float? pitchOverride = null)
    {
        if (!_sfxDict.TryGetValue(sfxId, out SFXEntry entry))
        {
            Debug.LogWarning($"[AudioManager] SFX '{sfxId}' no encontrado.");
            return;
        }

        float vol = entry.volume > 0f ? entry.volume : 1f;

        if (pitchOverride.HasValue || entry.randomizePitch)
        {
            // Usar Object Pool para no ensuciar el Pitch del canal principal
            AudioSource src = GetSFXPoolSource();
            src.pitch = pitchOverride.HasValue ? pitchOverride.Value : UnityEngine.Random.Range(0.85f, 1.15f);
            src.volume = vol;
            src.PlayOneShot(entry.clip);
        }
        else
        {
            _sfxSource.pitch = 1f; // Asegurar pitch normal
            _sfxSource.PlayOneShot(entry.clip, vol);
        }
    }

    /// <summary>
    /// Permite a otros scripts obtener un clip y sus ajustes desde la librería central
    /// (Ideal para reproducir sonidos que necesitan cortarse, como el riel de pesca).
    /// </summary>
    public bool TryGetSFXEntry(string sfxId, out SFXEntry entry)
    {
        return _sfxDict.TryGetValue(sfxId, out entry);
    }

    private AudioSource GetSFXPoolSource()
    {
        AudioSource src = _sfxPool.Dequeue();
        _sfxPool.Enqueue(src); // Lo mandamos al fondo de la cola
        return src;
    }

    // -------------------------------------------------------------------------
    // Opciones de Volumen
    // -------------------------------------------------------------------------
    public void SetMasterVolume(float linearValue) => ApplyVolume(MIXER_PARAM_MASTER, PREF_KEY_MASTER, linearValue);
    public void SetMusicVolume(float linearValue)  => ApplyVolume(MIXER_PARAM_MUSIC, PREF_KEY_MUSIC, linearValue);
    public void SetSFXVolume(float linearValue)    => ApplyVolume(MIXER_PARAM_SFX, PREF_KEY_SFX, linearValue);

    private void ApplyVolume(string mixerParam, string prefsKey, float linearValue)
    {
        linearValue = Mathf.Clamp(linearValue, 0.0001f, 1f);
        float dB = Mathf.Log10(linearValue) * 20f;
        if (_mainMixer != null)
            _mainMixer.SetFloat(mixerParam, dB);
        PlayerPrefs.SetFloat(prefsKey, linearValue);
    }

    private void LoadSavedVolumePreferences()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(PREF_KEY_MASTER, DEFAULT_VOLUME));
        SetMusicVolume(PlayerPrefs.GetFloat(PREF_KEY_MUSIC, DEFAULT_VOLUME));
        SetSFXVolume(PlayerPrefs.GetFloat(PREF_KEY_SFX, DEFAULT_VOLUME));
    }

    // -------------------------------------------------------------------------
    // Corrutinas y Helpers Internos
    // -------------------------------------------------------------------------
    private void PlayMusicWithCrossfade(AudioClip baseClip, AudioClip tensionClip, float fadeDuration = 1f)
    {
        if (_musicSourceBase.clip == baseClip && _musicSourceBase.isPlaying) return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        if (baseClip == null)
            _fadeCoroutine = StartCoroutine(FadeOutAndStop(fadeDuration));
        else if (!_musicSourceBase.isPlaying)
            _fadeCoroutine = StartCoroutine(FadeIn(baseClip, tensionClip, fadeDuration));
        else
            _fadeCoroutine = StartCoroutine(Crossfade(baseClip, tensionClip, fadeDuration));
    }

    private IEnumerator Crossfade(AudioClip baseClip, AudioClip tensionClip, float duration)
    {
        // Apagar lo actual
        yield return StartCoroutine(FadeVolumeRoutine(_musicSourceBase, _musicSourceBase.volume, 0f, duration));
        
        _musicSourceBase.clip = baseClip;
        _musicSourceTension.clip = tensionClip;
        
        _musicSourceBase.Play();
        if (tensionClip != null) _musicSourceTension.Play();

        // Subir base, dejar tensión en 0
        yield return StartCoroutine(FadeVolumeRoutine(_musicSourceBase, 0f, _musicTargetVolume, duration));
    }

    private IEnumerator FadeIn(AudioClip baseClip, AudioClip tensionClip, float duration)
    {
        _musicSourceBase.clip = baseClip;
        _musicSourceTension.clip = tensionClip;
        
        _musicSourceBase.volume = 0f;
        _musicSourceTension.volume = 0f;
        
        _musicSourceBase.Play();
        if (tensionClip != null) _musicSourceTension.Play();

        yield return StartCoroutine(FadeVolumeRoutine(_musicSourceBase, 0f, _musicTargetVolume, duration));
    }

    private IEnumerator FadeOutAndStop(float duration)
    {
        yield return StartCoroutine(FadeVolumeRoutine(_musicSourceBase, _musicSourceBase.volume, 0f, duration));
        _musicSourceBase.Stop();
        _musicSourceTension.Stop();
    }

    private IEnumerator FadeVolumeRoutine(AudioSource source, float startVol, float endVol, float duration)
    {
        if (duration <= 0f)
        {
            source.volume = endVol;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVol, endVol, elapsed / duration);
            yield return null;
        }
        source.volume = endVol;
    }

    private AudioSource CreateAudioSource(string sourceName, AudioMixerGroup mixerGroup, bool loop)
    {
        GameObject child = new GameObject(sourceName);
        child.transform.SetParent(transform);
        AudioSource source = child.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = mixerGroup;
        source.loop = loop;
        source.playOnAwake = false;
        return source;
    }

    private void BuildLookupDictionaries()
    {
        _sfxDict = new Dictionary<string, SFXEntry>(_sfxLibrary.Count);
        foreach (SFXEntry entry in _sfxLibrary)
        {
            // Setear volumen default por código si se olvidan de tocar el slider
            SFXEntry e = entry;
            if (e.volume <= 0f) e.volume = 1f; 
            if (!string.IsNullOrEmpty(e.id)) _sfxDict[e.id] = e;
        }

        _bgmDict = new Dictionary<string, AudioClip>(_bgmLibrary.Count);
        foreach (BGMEntry entry in _bgmLibrary)
        {
            if (!string.IsNullOrEmpty(entry.id)) _bgmDict[entry.id] = entry.clip;
        }
    }
}