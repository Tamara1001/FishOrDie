using UnityEngine;
using TMPro;

public class RoundTransitionUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Texto central que dirá '¡P1 FUE DEVORADO!'")]
    [SerializeField] private TMP_Text _eliminationMessage;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void OnEnable()
    {
        RoundManager.OnPlayerEliminated += HandlePlayerEliminated;
        
        // Limpiamos el texto por defecto al encender
        if (_eliminationMessage != null && string.IsNullOrEmpty(_eliminationMessage.text))
        {
            _eliminationMessage.text = "";
        }
    }

    private void OnDisable()
    {
        RoundManager.OnPlayerEliminated -= HandlePlayerEliminated;
        
        if (_eliminationMessage != null)
        {
            _eliminationMessage.text = "";
        }
    }

    // -------------------------------------------------------------------------
    // Handlers
    // -------------------------------------------------------------------------
    private void HandlePlayerEliminated(PlayerController loser)
    {
        if (_eliminationMessage == null) return;
        
        if (loser == null)
        {
            _eliminationMessage.text = "¡NADIE FUE DEVORADO!";
            _eliminationMessage.color = Color.white;
            return;
        }

        // ColorHex para el rich text (convierte Color a #RRGGBB)
        string colorHex = ColorUtility.ToHtmlStringRGB(loser.PlayerColor);
        
        // Mensaje con el nombre del jugador en su color
        _eliminationMessage.text = $"¡<color=#{colorHex}>{loser.gameObject.name}</color>\nFUE DEVORADO!";
    }
}
