using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestiona la pantalla intermedia entre el Menú Principal y el Gameplay.
/// Permite a los jugadores hacer "Drop-In" (unirse) simplemente presionando una tecla.
/// </summary>
public class UI_LobbyMenu : MonoBehaviour
{
    [Header("Lobby Config")]
    [Tooltip("Arrastrar acá las 6 filas de configuración creadas en la UI del LOBBY")]
    [SerializeField] private UI_PlayerConfigSlot[] _lobbySlots;

    [Header("Game Settings")]
    [Tooltip("Min: 0 (Fácil), Max: 2 (Difícil)")]
    [SerializeField] private UnityEngine.UI.Slider _difficultySlider;
    [SerializeField] private TMPro.TMP_Text _difficultyLabel;

    [Tooltip("Min: 0 (Poco), Max: 2 (Mucho)")]
    [SerializeField] private UnityEngine.UI.Slider _fishAmountSlider;
    [SerializeField] private TMPro.TMP_Text _fishAmountLabel;

    private void OnEnable()
    {
        MatchSettings.Load();

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

        RefreshLobbySlots();
    }

    private void OnDisable()
    {
        MatchSettings.Save();
        if (_difficultySlider != null) _difficultySlider.onValueChanged.RemoveListener(OnDifficultyChanged);
        if (_fishAmountSlider != null) _fishAmountSlider.onValueChanged.RemoveListener(OnFishAmountChanged);
    }

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

    private void Update()
    {
        // Ignorar si el usuario está escribiendo su nombre con el mouse
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null &&
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            foreach (var key in Keyboard.current.allKeys)
            {
                if (key.wasPressedThisFrame)
                {
                    TryJoinPlayer(key.name);
                    break;
                }
            }
        }
    }

    private void TryJoinPlayer(string keyName)
    {
        string binding = $"<Keyboard>/{keyName}";

        // 1. Verificar si la tecla ya está en uso por un jugador activo
        for (int i = 0; i < MatchSettings.MAX_PLAYERS; i++)
        {
            if (MatchSettings.PlayerActive[i] && MatchSettings.PlayerBindings[i].ToLower() == binding.ToLower())
            {
                return; // Ignorar tecla, ya está jugando
            }
        }

        // 2. Buscar el primer slot vacío para meter al jugador
        for (int i = 0; i < MatchSettings.MAX_PLAYERS; i++)
        {
            if (!MatchSettings.PlayerActive[i])
            {
                MatchSettings.PlayerActive[i] = true;
                MatchSettings.PlayerBindings[i] = binding;
                
                string cleanName = InputControlPath.ToHumanReadableString(binding, InputControlPath.HumanReadableStringOptions.OmitDevice);
                MatchSettings.PlayerNames[i] = $"Jugador {cleanName.ToUpper()}";
                
                MatchSettings.PlayerColors[i] = MatchSettings.GetFirstUnusedColor();
                
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_StartGame");
                
                RefreshLobbySlots();
                break;
            }
        }
    }

    public void RefreshLobbySlots()
    {
        if (_lobbySlots == null || _lobbySlots.Length == 0) return;

        for (int i = 0; i < _lobbySlots.Length; i++)
        {
            if (_lobbySlots[i] == null) continue;

            bool isActive = MatchSettings.PlayerActive[i];
            
            // Apaga el slot físico de UI si no está activo, revelando el "Presiona para unirte" de fondo
            _lobbySlots[i].gameObject.SetActive(isActive);

            if (isActive)
            {
                // Inyectar método para refrescar si alguien apreta la cruz [X]
                _lobbySlots[i].OnSlotStateChanged = RefreshLobbySlots;
                _lobbySlots[i].Initialize(i);
            }
        }
    }

    public void OnRemoveAllPlayersClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Cancel");
        
        for (int i = 0; i < MatchSettings.MAX_PLAYERS; i++)
        {
            MatchSettings.PlayerActive[i] = false;
        }
        
        RefreshLobbySlots();
    }
}
