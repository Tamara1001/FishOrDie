using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundHUD : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private TMP_Text _timerLabel;

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
        RoundManager.OnTimerTick       += HandleTimerTick;
        RoundManager.OnPlayerEliminated += HandlePlayerEliminated;
        RoundManager.OnScoreAdded      += HandleScoreAdded;
        RoundManager.OnRoundReset      += HandleRoundReset;
    }

    private void OnDisable()
    {
        RoundManager.OnTimerTick       -= HandleTimerTick;
        RoundManager.OnPlayerEliminated -= HandlePlayerEliminated;
        RoundManager.OnScoreAdded      -= HandleScoreAdded;
        RoundManager.OnRoundReset      -= HandleRoundReset;
    }

    private void Start()
    {
        BuildSlots();
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

        if (_scorePopupPrefab == null || slot == null) return;

        // Instanciamos el popup como hijo del slot (UI)
        GameObject popup = Instantiate(_scorePopupPrefab, slot.transform);
        
        // Lo levantamos un poquito desde el centro del slot
        if (popup.TryGetComponent(out RectTransform rt))
        {
            rt.anchoredPosition = new Vector2(0f, 50f); 
        }
        else
        {
            popup.transform.localPosition = new Vector3(0f, 50f, 0f);
        }

        if (popup.TryGetComponent(out ScorePopup scorePopup))
            scorePopup.Initialize(delta, player.PlayerColor);
    }

    private void HandlePlayerEliminated(PlayerController player)
    {
        PlayerHUDSlot slot = _slots.Find(s => s.Player == player);
        slot?.MarkEliminated();
    }

    private void HandleRoundReset()
    {
        foreach (PlayerHUDSlot slot in _slots)
            slot.RefreshScore();
    }
}
