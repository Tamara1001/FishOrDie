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

    private void OnEnable()
    {
        // Al encender el panel del Lobby, refrescamos visualmente los que ya estaban activos
        RefreshLobbySlots();
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
                
                MatchSettings.PlayerColors[i] = GetFirstUnusedColor();
                
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_StartGame");
                
                RefreshLobbySlots();
                break;
            }
        }
    }

    private Color GetFirstUnusedColor()
    {
        foreach (Color candidate in MatchSettings.AvailableColors)
        {
            bool isUsed = false;
            for (int k = 0; k < MatchSettings.MAX_PLAYERS; k++)
            {
                if (MatchSettings.PlayerActive[k] && MatchSettings.PlayerColors[k] == candidate)
                {
                    isUsed = true;
                    break;
                }
            }
            if (!isUsed) return candidate;
        }
        
        // Si (por algún motivo rarísimo) todos los colores de la paleta estuvieran usados, devolvemos uno al azar
        return MatchSettings.AvailableColors[Random.Range(0, MatchSettings.AvailableColors.Length)];
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
}
