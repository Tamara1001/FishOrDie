using UnityEngine;
using UnityEngine.UI;

public class UI_OptionsMenu : MonoBehaviour
{
    [Header("Volume Sliders")]
    [Tooltip("Min Value: 0.0001 — Max Value: 1")]
    [SerializeField] private Slider _masterSlider;

    [Tooltip("Min Value: 0.0001 — Max Value: 1")]
    [SerializeField] private Slider _musicSlider;

    [Tooltip("Min Value: 0.0001 — Max Value: 1")]
    [SerializeField] private Slider _sfxSlider;

    private void Start()
    {
        float savedMaster = PlayerPrefs.GetFloat(AudioManager.PREF_KEY_MASTER, 1f);
        float savedMusic  = PlayerPrefs.GetFloat(AudioManager.PREF_KEY_MUSIC,  1f);
        float savedSFX    = PlayerPrefs.GetFloat(AudioManager.PREF_KEY_SFX,    1f);

        if (_masterSlider != null) _masterSlider.value = savedMaster;
        if (_musicSlider  != null) _musicSlider.value  = savedMusic;
        if (_sfxSlider    != null) _sfxSlider.value    = savedSFX;

        if (_masterSlider != null) _masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        if (_musicSlider  != null) _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        if (_sfxSlider    != null) _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    private void OnDestroy()
    {
        if (_masterSlider != null) _masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);
        if (_musicSlider  != null) _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        if (_sfxSlider    != null) _sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
    }

    private void OnMasterSliderChanged(float value) => AudioManager.Instance.SetMasterVolume(value);
    private void OnMusicSliderChanged(float value)  => AudioManager.Instance.SetMusicVolume(value);
    private void OnSFXSliderChanged(float value)    => AudioManager.Instance.SetSFXVolume(value);
}
