using UnityEngine;
using TMPro;

public class UI_TutorialMenu : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private TextMeshProUGUI _tutorialText;
    
    private string[] _dialogues = new string[]
    {
        "Presioná tu tecla para lanzar la boya. ¡Ese es el único botón que vas a necesitar!",
        "Los peces ROJOS son más difíciles pero pagan fortunas.\nLos VERDES son fáciles pero baratos. ¡Cuidado de no pescar BASURA!",
        "Aguardá paciente a que pique... ¡y cuando veas el aviso en tu boya, reaccioná rápido!",
        "Mantené presionado para subir la fuerza, y soltá para bajarla.\n¡Guiate leyendo las instrucciones de colores en la pantalla!",
        "El tiempo corre. ¡Al terminar la ronda, el que tenga menos dinero será devorado!"
    };
    
    private int _currentIndex = 0;
    private System.Action _onTutorialComplete;
    private InteractiveTutorialStage _stageController;

    // Guardamos los valores originales del RectTransform para restaurarlos al salir
    private Vector2 _origAnchorMin;
    private Vector2 _origAnchorMax;
    private Vector2 _origOffsetMin;
    private Vector2 _origOffsetMax;
    private Vector2 _origPivot;
    private bool _savedOriginals = false;

    public void ShowTutorial(System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        if (_tutorialPanel != null) 
        {
            _tutorialPanel.SetActive(true);
            
            // Asegurar fondo negro oscuro (si tiene Image en el root)
            UnityEngine.UI.Image img = _tutorialPanel.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = new Color(0f, 0f, 0f, 0.92f);
        }
        
        // Crear o buscar el stage controller
        if (_stageController == null)
        {
            _stageController = gameObject.AddComponent<InteractiveTutorialStage>();
            _stageController.Initialize();
        }
        
        _onTutorialComplete = onComplete;
        _currentIndex = 0;
        UpdateText();
    }

    public void OnNextClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Click");
        _currentIndex++;
        if (_currentIndex >= _dialogues.Length)
        {
            EndTutorial();
        }
        else
        {
            UpdateText();
        }
    }

    public void OnSkipClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Cancel");
        EndTutorial();
    }

    private void UpdateText()
    {
        if (_tutorialText != null) _tutorialText.text = _dialogues[_currentIndex];
        
        if (_stageController != null)
        {
            _stageController.PlaySlide(_currentIndex);
        }
    }

    private void EndTutorial()
    {
        if (_stageController != null) _stageController.ClearStage();

        if (_tutorialPanel != null) _tutorialPanel.SetActive(false);
        gameObject.SetActive(false);
        
        // Disparar el inicio del juego si venimos del Lobby
        _onTutorialComplete?.Invoke();
        _onTutorialComplete = null;
    }
}
