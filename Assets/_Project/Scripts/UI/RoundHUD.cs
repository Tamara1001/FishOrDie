using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundHUD : MonoBehaviour
{
    [Header("Top HUD")]
    [SerializeField] private TMP_Text _timerLabel;
    [SerializeField] private TMP_Text _roundLabel;

    [Header("Player Slots")]
    [SerializeField] private GameObject _playerSlotPrefab;
    [SerializeField] private Transform _playersContainer;

    [Header("Score Popup")]
    [Tooltip("Prefab de mundo (TextMeshPro 3D). Déjalo vacío para desactivar los popups.")]
    [SerializeField] private GameObject _scorePopupPrefab;

    [Header("References")]
    [SerializeField] private PlayerSpawner _playerSpawner;

    private readonly List<PlayerHUDSlot> _slots = new();

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void OnEnable()
    {
        RoundManager.OnTimerTick += HandleTimerTick;
        RoundManager.OnScoreAdded += HandleScoreAdded;
        RoundManager.OnPlayerEliminated += HandlePlayerEliminated;
        RoundManager.OnRoundReset += HandleRoundReset;
        RoundManager.OnFishCatchDetails += HandleFishCatchDetails;
    }

    private void OnDisable()
    {
        RoundManager.OnTimerTick -= HandleTimerTick;
        RoundManager.OnScoreAdded -= HandleScoreAdded;
        RoundManager.OnPlayerEliminated -= HandlePlayerEliminated;
        RoundManager.OnRoundReset -= HandleRoundReset;
        RoundManager.OnFishCatchDetails -= HandleFishCatchDetails;
    }

    private void Start()
    {
        BuildSlots();
        UpdateRoundLabel();
    }

    private void UpdateRoundLabel()
    {
        if (_roundLabel != null)
        {
            _roundLabel.text = $"RONDA {RoundManager.CurrentRoundNumber}";
        }
    }

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------
    private void BuildSlots()
    {
        if (_playerSpawner == null)
        {
            Debug.LogError("[RoundHUD] PlayerSpawner reference is not assigned.");
            return;
        }

        if (_playerSlotPrefab == null || _playersContainer == null)
        {
            Debug.LogError("[RoundHUD] PlayerSlotPrefab or PlayersContainer is not assigned.");
            return;
        }

        foreach (PlayerController player in _playerSpawner.SpawnedPlayers)
        {
            GameObject slotGO = Instantiate(_playerSlotPrefab, _playersContainer, false);
            slotGO.transform.localScale = Vector3.one;

            if (!slotGO.TryGetComponent(out PlayerHUDSlot slot))
            {
                Debug.LogError("[RoundHUD] PlayerSlotPrefab is missing a PlayerHUDSlot component.");
                continue;
            }

            slot.Initialize(player);
            _slots.Add(slot);
        }
    }

    // -------------------------------------------------------------------------
    // Event Handlers
    // -------------------------------------------------------------------------
    private void HandleFishCatchDetails(PlayerController player, string fishName, int size, int weight, int score)
    {
        PlayerHUDSlot slot = _slots.Find(s => s.Player == player);
        slot?.ShowCatchBanner(fishName, size, weight, score);
    }

    private void HandleTimerTick(float remaining)
    {
        if (_timerLabel == null) return;

        _timerLabel.text  = Mathf.CeilToInt(remaining).ToString();
        _timerLabel.color = remaining <= 5f ? Color.red : Color.white;
    }

    private void HandleScoreAdded(PlayerController player, int delta)
    {
        PlayerHUDSlot slot = _slots.Find(s => s.Player == player);
        slot?.RefreshScore();

        if (_scorePopupPrefab == null || player == null) return;

        // Calculamos la posición en el mundo 3D/2D
        Vector3 worldPos = player.PopupSpawnPoint != player.transform 
            ? player.PopupSpawnPoint.position 
            : player.transform.position + Vector3.up * 1.5f;

        // Lo instanciamos TOTALMENTE SUELTO en el mundo real (sin parent)
        GameObject popup = Instantiate(_scorePopupPrefab, worldPos, Quaternion.identity);

        if (popup.TryGetComponent(out ScorePopup scorePopup))
        {
            scorePopup.Initialize(delta, player.PlayerColor);
        }
    }

    private void HandlePlayerEliminated(PlayerController player)
    {
        PlayerHUDSlot slot = _slots.Find(s => s.Player == player);
        slot?.MarkEliminated();
    }

    private void HandleRoundReset()
    {
        UpdateRoundLabel();
        foreach (PlayerHUDSlot slot in _slots)
        {
            slot.RefreshScore();
        }
    }
}
