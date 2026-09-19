using UnityEngine;
using TMPro;

public class UI_TutorialMenu : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private TextMeshProUGUI _tutorialText;
    
    private string[] _dialogues = new string[]
    {
        "¡Bienvenidos al Río Paraná!\nEl objetivo es atrapar peces para sumar puntos.",
        "El jugador con MENOS puntos al terminar el tiempo será DEVORADO por el Monstruo.",
        "Cuando un pez nade a la superficie y toque tu boya, ¡Apretá tu tecla rápidamente para engancharlo!",
        "Luego, presioná repetidamente para mantener la barra verde llena y capturarlo.",
        "¡CUIDADO CON LA BASURA! Si ves una bota o botella flotando, no intentes pescarla o perderás puntos instantáneamente.",
        "¡Buena suerte, y que no te coman!"
    };
    
    private int _currentIndex = 0;

    private void Start()
    {
        if (PlayerPrefs.GetInt("TutorialVisto", 0) == 1)
        {
            if (_tutorialPanel != null) _tutorialPanel.SetActive(false);
            return;
        }

        GameManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Playing && PlayerPrefs.GetInt("TutorialVisto", 0) == 0)
        {
            ShowTutorial();
        }
    }

    private void ShowTutorial()
    {
        if (_tutorialPanel != null) _tutorialPanel.SetActive(true);
        _currentIndex = 0;
        UpdateText();
        Time.timeScale = 0f; // Pausar
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
        PlayerPrefs.SetInt("TutorialVisto", 1);
        PlayerPrefs.Save();
        
        if (_tutorialPanel != null) _tutorialPanel.SetActive(false);
        
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            Time.timeScale = 1f;
        }
    }
}
