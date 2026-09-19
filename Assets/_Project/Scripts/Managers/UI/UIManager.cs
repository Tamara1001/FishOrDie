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
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject pausePanel;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        // En WebGL, salir de pantalla completa hace que el canvas pierda el foco de la página.
        // Si no activamos esto, Unity pausa el renderizado (pantalla negra) hasta recibir un click.
        Application.runInBackground = true;
    }

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
            _victoryText.text = $"¡<color=#{colorHex}>{winner.PlayerName}</color>\nSOBREVIVIÓ AL PARANÁ!";
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
        _isTransitioning = false;
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
        lobbyPanel?.SetActive(false);
        optionsPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Public Button Callbacks
    // -------------------------------------------------------------------------
    public void OnPlayClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Click");
        // En lugar de iniciar el juego directo, abrimos el Lobby
        lobbyPanel?.SetActive(true);
    }
    
    public void OnCloseLobbyClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Cancel");
        MatchSettings.Save(); // Guarda si el jugador armó un lobby y se arrepintió, mantiene sincronía
        lobbyPanel?.SetActive(false);
    }
    
    private bool _isTransitioning = false;

    public void OnLobbyStartGameClicked()
    {
        if (_isTransitioning) return;

        if (MatchSettings.GetActivePlayerCount() < 2)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Cancel");
            Debug.LogWarning("[Lobby] No puedes iniciar con menos de 2 jugadores.");
            return; // Bloquea el inicio de la partida
        }

        // Sincronizar y guardar para que el Menú de Ajustes lo adopte como default
        MatchSettings.Save();

        _isTransitioning = true; // Bloquea los botones del menú principal durante el fade

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_StartGame");
        lobbyPanel?.SetActive(false);
        GameManager.Instance.StartNewGame();
    }
    
    public void OnResumeButtonClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Click");
        GameManager.Instance.ResumeFromPause();
    }
    
    public void OnRestartButtonClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_StartGame");
        GameManager.Instance.StartNewGame();
    }
    
    public void OnReturnToMenuClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Cancel");
        GameManager.Instance.ReturnToMainMenu();
    }
    
    public void OnPauseButtonClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Click");
        GameManager.Instance.ChangeState(GameManager.GameState.Paused);
    }
    
    public void OnOptionsClicked()
    {
        if (_isTransitioning) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Click");
        optionsPanel?.SetActive(true);
    }
    
    public void OnCloseOptionsClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Cancel");
        optionsPanel?.SetActive(false);
    }

    public void OnCreditsClicked()       
    {
        if (_isTransitioning) return;
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
#elif UNITY_WEBGL
        // En navegadores no podemos cerrar la aplicación, así que quitamos la pantalla completa.
        Screen.fullScreen = false;
        Debug.Log("[UIManager] Saliendo de pantalla completa (WebGL).");
#else
        Application.Quit();
#endif
    }
}