using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static GameManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // FSM
    // -------------------------------------------------------------------------
    public enum GameState { MainMenu, Playing, RoundTransition, Paused, GameOver, Victory }

    public GameState CurrentState { get; private set; }
    public bool HasActiveSession { get; private set; }

    public static event Action<GameState> OnStateChanged;

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------
    private float _sessionTimer;
    private GameState _stateBeforePause;
    private bool _pendingRestart = false;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentState = GameState.MainMenu;
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_pendingRestart) return;

        _pendingRestart  = false;
        _sessionTimer    = 0f;
        HasActiveSession = true;

        Time.timeScale = 1f;
        ChangeState(GameState.Playing);
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
            _sessionTimer += Time.deltaTime;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
        {
            Debug.LogWarning($"[GameManager] Already in state {newState}. Ignored.");
            return;
        }

        switch (newState)
        {
            case GameState.Paused:
                _stateBeforePause = CurrentState;
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
            case GameState.Victory:
                HasActiveSession = false;
                Time.timeScale = 0f;
                break;
            default:
                Time.timeScale = 1f;
                break;
        }

        GameState previous = CurrentState;
        CurrentState = newState;

        Debug.Log($"[GameManager] {previous} → {CurrentState}");
        OnStateChanged?.Invoke(CurrentState);
    }

    public void StartNewGame()
    {
        StartCoroutine(StartNewGameRoutine());
    }

    private System.Collections.IEnumerator StartNewGameRoutine()
    {
        Time.timeScale = 1f;

        if (UI_ScreenFader.Instance != null)
        {
            UI_ScreenFader.Instance.FadeTo(1f, 0.5f);
            yield return new WaitForSecondsRealtime(0.5f);
        }

        _pendingRestart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ContinueGame()
    {
        if (!HasActiveSession)
        {
            Debug.LogWarning("[GameManager] ContinueGame called with no active session. Ignored.");
            return;
        }
        ChangeState(GameState.Playing);
    }

    public void ResumeFromPause()
    {
        if (CurrentState != GameState.Paused)
        {
            Debug.LogWarning("[GameManager] ResumeFromPause called but the game is not paused.");
            return;
        }
        ChangeState(_stateBeforePause);
    }

    public void ReturnToMainMenu()
    {
        StartCoroutine(ReturnToMenuRoutine());
    }

    private System.Collections.IEnumerator ReturnToMenuRoutine()
    {
        if (UI_ScreenFader.Instance != null)
        {
            UI_ScreenFader.Instance.FadeTo(1f, 0.5f);
            yield return new WaitForSecondsRealtime(0.5f);
        }

        _sessionTimer = 0f;
        Time.timeScale = 1f;
        ChangeState(GameState.MainMenu);
        
        if (UI_ScreenFader.Instance != null)
        {
            UI_ScreenFader.Instance.FadeTo(0f, 0.5f);
        }
    }

    public float SessionTime => _sessionTimer;
}