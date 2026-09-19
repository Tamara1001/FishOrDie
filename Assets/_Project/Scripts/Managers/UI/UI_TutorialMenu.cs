using UnityEngine;
using TMPro;

public class UI_TutorialMenu : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private TextMeshProUGUI _tutorialText;
    
    private string[] _dialogues = new string[]
    {
        "¡Bienvenidos a Fish or Die!\nEl objetivo es atrapar peces y sobrevivir en las peligrosas aguas del Río Paraná.\n\nAl finalizar el tiempo, el jugador con MENOS dinero será DEVORADO por el Monstruo del río.",
        "Atento a tu pescador: cuando empiece a forcejear y aparezca el triángulo sobre la boya, ¡apretá rápidamente tu tecla para enganchar el pez!\n\nLuego, presionala repetidamente para mantener el pez en la zona verde y capturarlo.",
        "¡OJO CON LA BASURA! Si pican botas o botellas viejas NO aprietes nada. Si las pescás te restarán dinero instantáneamente.\n\nPeces como el Dorado dan mucho dinero pero son agresivos. ¡Buena suerte, y que no te coman!"
    };
    
    private int _currentIndex = 0;
    private System.Action _onTutorialComplete;

    public void ShowTutorial(System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        if (_tutorialPanel != null) _tutorialPanel.SetActive(true);
        
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
    }

    private void EndTutorial()
    {
        if (_tutorialPanel != null) _tutorialPanel.SetActive(false);
        gameObject.SetActive(false); // Apagar el panel raíz también
        
        // Disparar el inicio del juego si venimos del Lobby
        _onTutorialComplete?.Invoke();
        _onTutorialComplete = null;
    }
}
