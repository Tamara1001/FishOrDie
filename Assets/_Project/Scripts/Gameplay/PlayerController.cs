using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
[DefaultExecutionOrder(0)]
public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Public State
    // -------------------------------------------------------------------------
    public int PlayerID    { get; private set; }
    public int CurrentScore { get; private set; }
    public Color PlayerColor { get; private set; }

    public SkillCheck CurrentSkillCheck { get; private set; }

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------
    public event Action<PlayerController>           OnFishAttempt;
    public event Action<PlayerController, FishData> OnFishCaught;

    // -------------------------------------------------------------------------
    // Input
    // -------------------------------------------------------------------------
    private InputAction _fishAction;

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------
    private SpriteRenderer _spriteRenderer;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        _fishAction?.Disable();
        _fishAction?.Dispose();
    }

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------
    public void Initialize(int id, string bindingPath, Color color)
    {
        PlayerID    = id;
        PlayerColor = color;
        _spriteRenderer.color = color;
        gameObject.name = $"Player_{id + 1}";

        _fishAction = new InputAction(
            name:    $"Player_{id + 1}_Fish",
            type:    InputActionType.Button,
            binding: bindingPath
        );
        _fishAction.Enable();
    }

    // -------------------------------------------------------------------------
    // Remapping API — call this to change the key at runtime
    // -------------------------------------------------------------------------
    public void RemapKey(string newBindingPath)
    {
        _fishAction.Disable();
        _fishAction.ApplyBindingOverride(0, newBindingPath);
        _fishAction.Enable();
    }

    // -------------------------------------------------------------------------
    // Game Loop
    // -------------------------------------------------------------------------
    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (_fishAction == null) return;

        if (CurrentSkillCheck != null && CurrentSkillCheck.IsActive)
        {
            CurrentSkillCheck.Tick(Time.deltaTime, _fishAction.IsPressed());
            return;
        }

        if (_fishAction.WasPressedThisFrame())
            OnFishAttempt?.Invoke(this);
    }

    // -------------------------------------------------------------------------
    // Skill Check API
    // -------------------------------------------------------------------------
    public void StartSkillCheck(FishData fish)
    {
        if (CurrentSkillCheck != null && CurrentSkillCheck.IsActive) return;
        CurrentSkillCheck = new SkillCheck(fish, HandleSkillCheckCaught, HandleSkillCheckEscaped);
    }

    private void HandleSkillCheckCaught(FishData fish)
    {
        CurrentSkillCheck = null;
        OnFishCaught?.Invoke(this, fish);
    }

    private void HandleSkillCheckEscaped()
    {
        CurrentSkillCheck = null;
        Debug.Log($"[{gameObject.name}] ¡El pez escapó!");
    }

    // -------------------------------------------------------------------------
    // Score API
    // -------------------------------------------------------------------------
    public void AddScore(int amount)
    {
        CurrentScore += amount;
        Debug.Log($"[{gameObject.name}] Score: {CurrentScore} (+{amount})");
    }

    public void ResetScore() => CurrentScore = 0;

    // -------------------------------------------------------------------------
    // Elimination
    // -------------------------------------------------------------------------
    public void Eliminate()
    {
        CurrentSkillCheck = null;
        _fishAction?.Disable();
        Debug.Log($"[{gameObject.name}] ¡Devorado por el Monstruo del Paraná!");
        gameObject.SetActive(false);
    }
}
