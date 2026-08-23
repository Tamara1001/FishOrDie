using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class UI_PlayerConfigSlot : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private Image _colorImage;
    [SerializeField] private Button _colorButton;
    [SerializeField] private Button _keyButton;
    [SerializeField] private TMP_Text _keyText;
    
    [Tooltip("Opcional: Imagen que muestra al jugador (ej: sprite de un gato) para teñirlo del color elegido.")]
    [SerializeField] private Image _playerPreviewImage;

    private int _playerIndex;
    private int _currentColorIndex = 0;
    private InputAction _tempAction;
    
    public System.Action OnSlotStateChanged;

    public void Initialize(int index)
    {
        _playerIndex = index;
        
        // Cargar datos actuales de MatchSettings
        _nameInput.text = MatchSettings.PlayerNames[index];
        
        _colorImage.color = MatchSettings.PlayerColors[index];
        if (_playerPreviewImage != null) _playerPreviewImage.color = MatchSettings.PlayerColors[index];
        UpdateKeyText(MatchSettings.PlayerBindings[index]);

        // Asegurarnos de que el índice de color coincida más o menos (o arrancar desde el 0 de la paleta)
        _currentColorIndex = index % MatchSettings.AvailableColors.Length;

        // Limpiar y asignar listeners
        _nameInput.onValueChanged.RemoveAllListeners();
        _nameInput.onValueChanged.AddListener(OnNameChanged);

        _colorButton.onClick.RemoveAllListeners();
        _colorButton.onClick.AddListener(CycleColor);

        _keyButton.onClick.RemoveAllListeners();
        _keyButton.onClick.AddListener(StartRebind);
    }

    private void OnNameChanged(string newName)
    {
        MatchSettings.PlayerNames[_playerIndex] = newName;
    }

    private void CycleColor()
    {
        _currentColorIndex = (_currentColorIndex + 1) % MatchSettings.AvailableColors.Length;
        Color newColor = MatchSettings.AvailableColors[_currentColorIndex];
        
        _colorImage.color = newColor;
        if (_playerPreviewImage != null) _playerPreviewImage.color = newColor;
        
        MatchSettings.PlayerColors[_playerIndex] = newColor;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Click");
    }

    private InputActionRebindingExtensions.RebindingOperation _rebindOperation;

    private void OnDisable()
    {
        _rebindOperation?.Cancel();
        _rebindOperation?.Dispose();
        _rebindOperation = null;

        _tempAction?.Dispose();
        _tempAction = null;
    }

    private void StartRebind()
    {
        // Desactivar botón temporalmente para que no hagan doble clic
        _keyButton.interactable = false;
        _keyText.text = "...";
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Click");

        // Cancelamos cualquier operación previa por seguridad
        _rebindOperation?.Cancel();
        _rebindOperation?.Dispose();

        // Creamos una acción temporal solo para escuchar la próxima tecla
        _tempAction = new InputAction(type: InputActionType.Button, binding: MatchSettings.PlayerBindings[_playerIndex]);
        
        _rebindOperation = _tempAction.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse") // No permitimos clics del mouse como controles
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => {
                string newBinding = operation.action.bindings[0].effectivePath;
                
                // Verificar si la tecla ya está en uso por OTRO jugador activo
                bool isDuplicate = false;
                for (int k = 0; k < MatchSettings.MAX_PLAYERS; k++)
                {
                    if (k != _playerIndex && MatchSettings.PlayerActive[k] && MatchSettings.PlayerBindings[k].ToLower() == newBinding.ToLower())
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (isDuplicate)
                {
                    // Rechazar el cambio de tecla
                    if (this != null && gameObject != null && _keyButton != null)
                    {
                        UpdateKeyText(MatchSettings.PlayerBindings[_playerIndex]);
                        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Cancel"); // Sonido de error
                        _keyButton.interactable = true;
                    }
                }
                else
                {
                    // Aceptar el cambio de tecla
                    MatchSettings.PlayerBindings[_playerIndex] = newBinding;
                    
                    if (this != null && gameObject != null && _keyButton != null)
                    {
                        UpdateKeyText(newBinding);
                        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_InputBind");
                        _keyButton.interactable = true;
                    }
                }
                
                operation.Dispose();
                _tempAction?.Dispose();
            })
            .OnCancel(operation => {
                if (this != null && gameObject != null && _keyButton != null)
                {
                    UpdateKeyText(MatchSettings.PlayerBindings[_playerIndex]);
                    _keyButton.interactable = true;
                }
                operation.Dispose();
                _tempAction?.Dispose();
            });
        
        _rebindOperation.Start();
    }

    private void UpdateKeyText(string bindingPath)
    {
        // Convierte "<Keyboard>/a" a algo legible como "A"
        string cleanName = InputControlPath.ToHumanReadableString(
            bindingPath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            
        _keyText.text = cleanName.ToUpper();
    }

    /// <summary>
    /// Llama a esta función desde un Botón (ej. una crucecita [X] en la UI del slot)
    /// para quitar a este jugador de la partida.
    /// </summary>
    public void LeaveLobby()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Cancel");
        
        MatchSettings.RemovePlayerAndShift(_playerIndex);
        
        OnSlotStateChanged?.Invoke();
    }
}
