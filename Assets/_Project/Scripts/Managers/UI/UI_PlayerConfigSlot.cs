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

    private int _playerIndex;
    private int _currentColorIndex = 0;
    private InputAction _tempAction;

    // Paleta de colores para iterar cuando se hace clic en el color
    private readonly Color[] _availableColors = { 
        Color.cyan, Color.red, Color.yellow, Color.green, 
        Color.magenta, Color.white, new Color(1f, 0.5f, 0f), 
        new Color(0.5f, 0f, 1f), Color.blue 
    };

    public void Initialize(int index)
    {
        _playerIndex = index;
        
        // Cargar datos actuales de MatchSettings
        _nameInput.text = MatchSettings.PlayerNames[index];
        _colorImage.color = MatchSettings.PlayerColors[index];
        UpdateKeyText(MatchSettings.PlayerBindings[index]);

        // Asegurarnos de que el índice de color coincida más o menos (o arrancar desde el 0 de la paleta)
        _currentColorIndex = index % _availableColors.Length;

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
        _currentColorIndex = (_currentColorIndex + 1) % _availableColors.Length;
        Color newColor = _availableColors[_currentColorIndex];
        
        _colorImage.color = newColor;
        MatchSettings.PlayerColors[_playerIndex] = newColor;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Click");
    }

    private void StartRebind()
    {
        // Desactivar botón temporalmente para que no hagan doble clic
        _keyButton.interactable = false;
        _keyText.text = "...";
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Click");

        // Creamos una acción temporal solo para escuchar la próxima tecla
        _tempAction = new InputAction(type: InputActionType.Button, binding: MatchSettings.PlayerBindings[_playerIndex]);
        
        var rebindOperation = _tempAction.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse") // No permitimos clics del mouse como controles
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => {
                string newBinding = operation.action.bindings[0].effectivePath;
                MatchSettings.PlayerBindings[_playerIndex] = newBinding;
                
                UpdateKeyText(newBinding);
                
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_InputBind");
                
                operation.Dispose();
                _tempAction.Dispose();
                _keyButton.interactable = true;
            })
            .OnCancel(operation => {
                UpdateKeyText(MatchSettings.PlayerBindings[_playerIndex]);
                operation.Dispose();
                _tempAction.Dispose();
                _keyButton.interactable = true;
            });
        
        rebindOperation.Start();
    }

    private void UpdateKeyText(string bindingPath)
    {
        // Convierte "<Keyboard>/a" a algo legible como "A"
        string cleanName = InputControlPath.ToHumanReadableString(
            bindingPath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            
        _keyText.text = cleanName.ToUpper();
    }
}
