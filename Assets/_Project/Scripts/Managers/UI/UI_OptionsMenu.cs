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

    [Header("Player Settings")]
    [SerializeField] private Slider _playerCountSlider;
    [SerializeField] private TMPro.TMP_Text _playerCountLabel;

    [Header("Game Settings")]
    [Tooltip("Min: 0 (Fácil), Max: 2 (Difícil)")]
    [SerializeField] private Slider _difficultySlider;
    [SerializeField] private TMPro.TMP_Text _difficultyLabel;

    [Tooltip("Min: 0 (Poco), Max: 2 (Mucho)")]
    [SerializeField] private Slider _fishAmountSlider;
    [SerializeField] private TMPro.TMP_Text _fishAmountLabel;
    
    [Header("Slots")]
    [Tooltip("Arrastrar acá las 6 filas de configuración creadas en la UI")]
    [SerializeField] private UI_PlayerConfigSlot[] _playerSlots;

    private void Start()
    {
        MatchSettings.Load();

        // --- AUDIO SETTINGS ---
        float savedMaster = PlayerPrefs.GetFloat(AudioManager.PREF_KEY_MASTER, 1f);
        float savedMusic  = PlayerPrefs.GetFloat(AudioManager.PREF_KEY_MUSIC,  1f);
        float savedSFX    = PlayerPrefs.GetFloat(AudioManager.PREF_KEY_SFX,    1f);

        if (_masterSlider != null) _masterSlider.value = savedMaster;
        if (_musicSlider  != null) _musicSlider.value  = savedMusic;
        if (_sfxSlider    != null) _sfxSlider.value    = savedSFX;

        if (_masterSlider != null) _masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        if (_musicSlider  != null) _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        if (_sfxSlider    != null) _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

        // --- PLAYER SETTINGS ---
        if (_playerCountSlider != null)
        {
            _playerCountSlider.minValue = 2;
            _playerCountSlider.maxValue = MatchSettings.MAX_PLAYERS;
            _playerCountSlider.wholeNumbers = true;
            
            _playerCountSlider.value = MatchSettings.PlayerCount;
            UpdatePlayerCountLabel(MatchSettings.PlayerCount);
            
            _playerCountSlider.onValueChanged.AddListener(OnPlayerCountChanged);
        }

        RefreshPlayerSlots();
    }

    private void OnEnable()
    {
        if (_playerCountSlider != null)
        {
            _playerCountSlider.onValueChanged.RemoveListener(OnPlayerCountChanged);
            _playerCountSlider.value = MatchSettings.PlayerCount;
            UpdatePlayerCountLabel(MatchSettings.PlayerCount);
            _playerCountSlider.onValueChanged.AddListener(OnPlayerCountChanged);
        }

        if (_difficultySlider != null)
        {
            _difficultySlider.onValueChanged.RemoveListener(OnDifficultyChanged);
            _difficultySlider.value = (int)MatchSettings.CurrentDifficulty;
            UpdateDifficultyLabel((int)MatchSettings.CurrentDifficulty);
            _difficultySlider.onValueChanged.AddListener(OnDifficultyChanged);
        }

        if (_fishAmountSlider != null)
        {
            _fishAmountSlider.onValueChanged.RemoveListener(OnFishAmountChanged);
            _fishAmountSlider.value = (int)MatchSettings.CurrentFishAmount;
            UpdateFishAmountLabel((int)MatchSettings.CurrentFishAmount);
            _fishAmountSlider.onValueChanged.AddListener(OnFishAmountChanged);
        }

        RefreshPlayerSlots();
    }

    private void OnDestroy()
    {
        MatchSettings.Save();
        if (_masterSlider != null) _masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);
        if (_musicSlider  != null) _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        if (_difficultySlider != null) _difficultySlider.onValueChanged.RemoveListener(OnDifficultyChanged);
        if (_fishAmountSlider != null) _fishAmountSlider.onValueChanged.RemoveListener(OnFishAmountChanged);
        if (_sfxSlider    != null) _sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
        
        if (_playerCountSlider != null) _playerCountSlider.onValueChanged.RemoveListener(OnPlayerCountChanged);
    }

    private void OnMasterSliderChanged(float value) => AudioManager.Instance.SetMasterVolume(value);
    private void OnMusicSliderChanged(float value)  => AudioManager.Instance.SetMusicVolume(value);
    private void OnSFXSliderChanged(float value)    => AudioManager.Instance.SetSFXVolume(value);

    // --- PLAYER CONFIG LOGIC ---
    private void OnPlayerCountChanged(float value)
    {
        int count = Mathf.RoundToInt(value);
        MatchSettings.PlayerCount = count;
        UpdatePlayerCountLabel(count);
        RefreshPlayerSlots();
    }

    private void UpdatePlayerCountLabel(int count)
    {
        if (_playerCountLabel != null)
            _playerCountLabel.text = $"{count} JUGADORES";
    }

    // --- GAME SETTINGS ---
    private void OnDifficultyChanged(float val)
    {
        int difficultyInt = Mathf.RoundToInt(val);
        MatchSettings.CurrentDifficulty = (MatchSettings.GameDifficulty)difficultyInt;
        UpdateDifficultyLabel(difficultyInt);
        MatchSettings.Save();
    }

    private void UpdateDifficultyLabel(int val)
    {
        if (_difficultyLabel == null) return;
        string[] labels = { "FÁCIL", "NORMAL", "DIFÍCIL" };
        _difficultyLabel.text = $"DIFICULTAD: {labels[Mathf.Clamp(val, 0, 2)]}";
    }

    private void OnFishAmountChanged(float val)
    {
        int amountInt = Mathf.RoundToInt(val);
        MatchSettings.CurrentFishAmount = (MatchSettings.FishAmount)amountInt;
        UpdateFishAmountLabel(amountInt);
        MatchSettings.Save();
    }

    private void UpdateFishAmountLabel(int val)
    {
        if (_fishAmountLabel == null) return;
        string[] labels = { "POCOS", "NORMAL", "MUCHOS" };
        _fishAmountLabel.text = $"PECES: {labels[Mathf.Clamp(val, 0, 2)]}";
    }

    public void RefreshPlayerSlots()
    {
        if (_playerSlots == null || _playerSlots.Length == 0) return;

        for (int i = 0; i < _playerSlots.Length; i++)
        {
            if (_playerSlots[i] == null) continue;

            bool isActive = MatchSettings.PlayerActive[i];
            _playerSlots[i].gameObject.SetActive(isActive);

            if (isActive)
            {
                // Hookup action para refrescar si se hace clic en LeaveLobby
                _playerSlots[i].OnSlotStateChanged = RefreshPlayerSlots;
                _playerSlots[i].Initialize(i);
            }
        }
    }
}
