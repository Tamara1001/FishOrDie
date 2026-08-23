using UnityEngine;

public class UIManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields — Panels
    // -------------------------------------------------------------------------
    [Header("Main UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playingHUDPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMPro.TMP_Text _victoryText; // NUEVO: Texto de victoria
    [SerializeField] private GameObject roundTransitionPanel;

    [Header("Overlay Panels")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject pausePanel;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void OnEnable()
    {
        GameManager.OnStateChanged += HandleStateChanged;
        RoundManager.OnVictoryEvent += HandleVictory;
    }

    private void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;
        RoundManager.OnVictoryEvent -= HandleVictory;
    }

    private void HandleVictory(PlayerController winner)
    {
        if (_victoryText == null) return;

        if (winner != null)
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(winner.PlayerColor);
            _victoryText.text = $"¡<color=#{colorHex}>{winner.gameObject.name}</color>\nSOBREVIVIÓ AL PARANÁ!";
        }
        else
        {
            _victoryText.text = "¡TODOS FUERON DEVORADOS!";
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            HandleStateChanged(GameManager.Instance.CurrentState);
        else
            ShowMainMenu();
    }

    // -------------------------------------------------------------------------
    // FSM Event Handler
    // -------------------------------------------------------------------------
    private void HandleStateChanged(GameManager.GameState newState)
    {
        CloseAllOverlayPanels();

        switch (newState)
        {
            case GameManager.GameState.MainMenu:        ShowMainMenu();        break;
            case GameManager.GameState.Playing:         ShowPlayingHUD();      break;
            case GameManager.GameState.RoundTransition: ShowRoundTransition(); break;
            case GameManager.GameState.Paused:          ShowPause();           break;
            case GameManager.GameState.Victory:         ShowVictory();         break;
            case GameManager.GameState.GameOver:        ShowVictory();         break;
            default:
                Debug.LogWarning($"[UIManager] Unhandled GameState: {newState}");
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Panel Control
    // -------------------------------------------------------------------------
    private void ShowMainMenu()
    {
        mainMenuPanel?.SetActive(true);
        playingHUDPanel?.SetActive(false);
        pausePanel?.SetActive(false);
        victoryPanel?.SetActive(false);
        roundTransitionPanel?.SetActive(false);
    }

    private void ShowPlayingHUD()
    {
        mainMenuPanel?.SetActive(false);
        playingHUDPanel?.SetActive(true);
        pausePanel?.SetActive(false);
        victoryPanel?.SetActive(false);
        roundTransitionPanel?.SetActive(false);
    }

    private void ShowRoundTransition()
    {
        mainMenuPanel?.SetActive(false);
        playingHUDPanel?.SetActive(true); // Mantener HUD visible
        pausePanel?.SetActive(false);
        victoryPanel?.SetActive(false);
        roundTransitionPanel?.SetActive(true);
    }

    private void ShowPause()
    {
        mainMenuPanel?.SetActive(false);
        playingHUDPanel?.SetActive(false);
        pausePanel?.SetActive(true);
        victoryPanel?.SetActive(false);
        roundTransitionPanel?.SetActive(false);
    }

    private void ShowVictory()
    {
        mainMenuPanel?.SetActive(false);
        playingHUDPanel?.SetActive(false);
        pausePanel?.SetActive(false);
        victoryPanel?.SetActive(true);
        roundTransitionPanel?.SetActive(false);
    }

    private void CloseAllOverlayPanels()
    {
        optionsPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Public Button Callbacks
    // -------------------------------------------------------------------------
    public void OnPlayClicked()          => GameManager.Instance.StartNewGame();
    public void OnResumeButtonClicked()  => GameManager.Instance.ResumeFromPause();
    public void OnRestartButtonClicked() => GameManager.Instance.StartNewGame();
    public void OnReturnToMenuClicked()  => GameManager.Instance.ReturnToMainMenu();
    public void OnPauseButtonClicked()   => GameManager.Instance.ChangeState(GameManager.GameState.Paused);
    public void OnOptionsClicked()       => optionsPanel?.SetActive(true);
    public void OnCloseOptionsClicked()  => optionsPanel?.SetActive(false);
    public void OnCreditsClicked()       
    {
        Debug.Log("[UIManager] Abriendo créditos");
        creditsPanel?.SetActive(true);
    }

    public void OnCloseCreditsClicked()  
    {
        Debug.Log("[UIManager] Cerrando créditos");
        creditsPanel?.SetActive(false);
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}