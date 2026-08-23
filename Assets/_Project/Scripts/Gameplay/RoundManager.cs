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
    [SerializeField] private RiverSpawner _riverSpawner;

    // -------------------------------------------------------------------------
    // Events (static so la UI puede suscribirse sin referencia directa)
    // -------------------------------------------------------------------------
    public static event Action<float> OnTimerTick;
    public static event Action<PlayerController> OnPlayerEliminated;
    public static event Action<PlayerController, int> OnScoreAdded;
    public static event Action<PlayerController, string, int, int, int> OnFishCatchDetails;
    public static event Action OnRoundReset;
    public static event Action<PlayerController> OnVictoryEvent;

    // -------------------------------------------------------------------------
    // Internal State
    // -------------------------------------------------------------------------
    public static int CurrentRoundNumber { get; private set; } = 1;

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
        CurrentRoundNumber = 1;
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

        if (_riverSpawner != null) 
            _riverSpawner.StartSpawning(_fishPool, _activePlayers);

        Debug.Log($"[RoundManager] Ronda iniciada con {_activePlayers.Count} jugador(es).");
    }

    private void EndRound()
    {
        _roundActive = false;
        UnsubscribeFromPlayers();

        // Forzar cancelación de cualquier pesca en curso
        foreach (PlayerController p in _activePlayers)
        {
            if (p != null) p.CancelSkillCheck();
        }
        
        if (_riverSpawner != null)
        {
            _riverSpawner.StopSpawning();
        }

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

        // Momento de tensión visual (mostrar quién fue devorado)
        yield return new WaitForSeconds(3f);

        // Fade a negro antes de cambiar la pantalla
        if (UI_ScreenFader.Instance != null)
        {
            UI_ScreenFader.Instance.FadeTo(1f, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }

        if (_activePlayers.Count <= 1)
        {
            DeclareVictory();
        }
        else
        {
            CurrentRoundNumber++;
            ResetAndStartNextRound();
        }

        // Fade a transparente cuando ya estemos en la nueva ronda o pantalla de victoria
        if (UI_ScreenFader.Instance != null)
        {
            UI_ScreenFader.Instance.FadeTo(0f, 0.5f);
        }
    }

    private void ResetAndStartNextRound()
    {
        foreach (PlayerController player in _activePlayers)
            player.ResetScore();

        if (_riverSpawner != null)
            _riverSpawner.ClearRiver();

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
        if (_riverSpawner == null || _riverSpawner.ActiveFishes.Count == 0) return;

        FishVisual hookedFish = null;
        float closestDist = float.MaxValue;
        float catchRadius = 2.0f; // Margen de error en unidades de Unity

        // Buscar el pez más cercano en el carril del jugador
        foreach (FishVisual fish in _riverSpawner.ActiveFishes)
        {
            // Solo lo podemos atrapar si subió a la superficie
            if (fish == null || !fish.IsCatchable) continue;

            float dist = Mathf.Abs(fish.transform.position.x - player.transform.position.x);
            if (dist <= catchRadius && dist < closestDist)
            {
                closestDist = dist;
                hookedFish = fish;
            }
        }

        if (hookedFish != null)
        {
            FishData data = hookedFish.Data;
            _riverSpawner.ActiveFishes.Remove(hookedFish);
            hookedFish.Hook();
            
            player.StartSkillCheck(data);
        }
        else
        {
            // El jugador apretó pero no había pez en la superficie en su carril
        }
    }

    private void OnFishCaught(PlayerController player, FishData fish)
    {
        if (fish == null) return;

        // --- Generación procedural de stats del pez ---
        int size = UnityEngine.Random.Range(fish.minSizeCm, fish.maxSizeCm + 1);
        
        // Regla de 3 para sacar el peso proporcional al tamaño
        float t = Mathf.InverseLerp(fish.minSizeCm, fish.maxSizeCm, size);
        int weight = Mathf.RoundToInt(Mathf.Lerp(fish.minWeightKg, fish.maxWeightKg, t));
        
        // Multiplicador de dinero
        int scoreToAdd = weight * fish.valuePerKg;
        // Evitar que valga 0 por si es muy chiquito
        if (scoreToAdd <= 0) scoreToAdd = 1;

        string fishName = fish.fishName;

        player.AddScore(scoreToAdd);
        OnScoreAdded?.Invoke(player, scoreToAdd);
        
        // Disparar cartel detallado para que lo agarre la UI
        OnFishCatchDetails?.Invoke(player, fishName, size, weight, scoreToAdd);

        Debug.Log($"[RoundManager] {player.gameObject.name} pescó: {fishName} | {size}cm | {weight}kg | +${scoreToAdd}");
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
