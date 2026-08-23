using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(10)]
public class RoundManager : MonoBehaviour
{
    [Header("Round Settings")]
    [Min(5f)]
    [SerializeField] private float _roundDuration = 30f;

    [Header("Fish Pool")]
    [Tooltip("Lista de peces disponibles para pescar. Si está vacía, se suma 1 punto por intento.")]
    [SerializeField] private FishData[] _fishPool;

    [Header("References")]
    [SerializeField] private PlayerSpawner _playerSpawner;

    // -------------------------------------------------------------------------
    // Events (static so la UI puede suscribirse sin referencia directa)
    // -------------------------------------------------------------------------
    public static event Action<float> OnTimerTick;
    public static event Action<PlayerController> OnPlayerEliminated;
    public static event Action<PlayerController, int> OnScoreAdded;
    public static event Action OnRoundReset;
    public static event Action<PlayerController> OnVictoryEvent;

    // -------------------------------------------------------------------------
    // Internal State
    // -------------------------------------------------------------------------
    private readonly List<PlayerController> _activePlayers = new();
    private float _timeRemaining;
    private bool _roundActive = false;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    // Nos suscribimos en Awake (antes de Start) para no perder el evento
    // que GameManager dispara desde SceneManager.sceneLoaded, el cual
    // ocurre entre OnEnable y Start.
    private void Awake()
    {
        GameManager.OnStateChanged += OnGameStateChanged;
    }

    private void Start()
    {
        if (_playerSpawner == null)
        {
            Debug.LogError("[RoundManager] PlayerSpawner reference is not assigned.");
            return;
        }

        // Seguridad: si la escena arrancó directamente en estado Playing
        // (ej. en el editor sin pasar por MainMenu), iniciamos la ronda aquí.
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameManager.GameState.Playing &&
            !_roundActive)
        {
            StartRound();
        }
    }

    private void OnDisable()
    {
        GameManager.OnStateChanged -= OnGameStateChanged;
        UnsubscribeFromPlayers();
    }

    // -------------------------------------------------------------------------
    // Game Loop
    // -------------------------------------------------------------------------
    private void Update()
    {
        if (!_roundActive) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        _timeRemaining -= Time.deltaTime;
        OnTimerTick?.Invoke(Mathf.Max(_timeRemaining, 0f));

        if (_timeRemaining <= 0f)
            EndRound();
    }

    // -------------------------------------------------------------------------
    // Round Flow
    // -------------------------------------------------------------------------
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Playing && !_roundActive)
            StartRound();
    }

    private void StartRound()
    {
        _activePlayers.Clear();

        foreach (PlayerController p in _playerSpawner.SpawnedPlayers)
        {
            if (p != null && p.gameObject.activeSelf)
                _activePlayers.Add(p);
        }

        SubscribeToPlayers();

        _timeRemaining = _roundDuration;
        _roundActive   = true;

        Debug.Log($"[RoundManager] Ronda iniciada con {_activePlayers.Count} jugador(es).");
    }

    private void EndRound()
    {
        _roundActive = false;
        UnsubscribeFromPlayers();
        StartCoroutine(RoundTransitionRoutine());
    }

    private System.Collections.IEnumerator RoundTransitionRoutine()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.RoundTransition);

        PlayerController loser = FindLowestScoringPlayer();
        if (loser != null)
        {
            loser.Eliminate();
            _activePlayers.Remove(loser);
            OnPlayerEliminated?.Invoke(loser);
        }
        else
        {
            Debug.LogWarning("[RoundManager] No se encontró un perdedor. ¿Lista de jugadores vacía?");
        }

        // Momento de tensión visual
        yield return new WaitForSeconds(3f);

        if (_activePlayers.Count <= 1)
        {
            DeclareVictory();
        }
        else
        {
            ResetAndStartNextRound();
        }
    }

    private void ResetAndStartNextRound()
    {
        foreach (PlayerController player in _activePlayers)
            player.ResetScore();

        OnRoundReset?.Invoke();
        
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
        StartRound();
    }

    private void DeclareVictory()
    {
        PlayerController winner = _activePlayers.Count == 1 ? _activePlayers[0] : null;

        if (winner != null)
            Debug.Log($"[RoundManager] ¡Ganador: {winner.gameObject.name}!");
        else
            Debug.Log("[RoundManager] No quedan jugadores.");

        OnVictoryEvent?.Invoke(winner);
        GameManager.Instance.ChangeState(GameManager.GameState.Victory);
    }

    // -------------------------------------------------------------------------
    // Score Logic
    // -------------------------------------------------------------------------
    private void OnFishAttempt(PlayerController player)
    {
        FishData fish = (_fishPool != null && _fishPool.Length > 0)
            ? _fishPool[UnityEngine.Random.Range(0, _fishPool.Length)]
            : null;

        player.StartSkillCheck(fish);
    }

    private void OnFishCaught(PlayerController player, FishData fish)
    {
        int scoreToAdd   = fish != null ? fish.scoreValue : 1;
        string fishName  = fish != null ? fish.fishName   : "Pez desconocido";

        player.AddScore(scoreToAdd);
        OnScoreAdded?.Invoke(player, scoreToAdd);
        Debug.Log($"[RoundManager] {player.gameObject.name} pescó un {fishName}! (+{scoreToAdd})");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
    private PlayerController FindLowestScoringPlayer()
    {
        PlayerController loser = null;

        foreach (PlayerController player in _activePlayers)
        {
            if (loser == null ||
                player.CurrentScore < loser.CurrentScore ||
               (player.CurrentScore == loser.CurrentScore && player.PlayerID > loser.PlayerID))
            {
                loser = player;
            }
        }

        return loser;
    }

    private void SubscribeToPlayers()
    {
        UnsubscribeFromPlayers();
        foreach (PlayerController p in _activePlayers)
        {
            p.OnFishAttempt += OnFishAttempt;
            p.OnFishCaught  += OnFishCaught;
        }
    }

    private void UnsubscribeFromPlayers()
    {
        foreach (PlayerController p in _activePlayers)
        {
            if (p == null) continue;
            p.OnFishAttempt -= OnFishAttempt;
            p.OnFishCaught  -= OnFishCaught;
        }
    }
}
