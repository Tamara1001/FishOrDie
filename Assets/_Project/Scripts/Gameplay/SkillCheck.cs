using System;
using UnityEngine;

public class SkillCheck
{
    // -------------------------------------------------------------------------
    // Read-only state exposed to the UI panel
    // -------------------------------------------------------------------------
    public float FishPosition  { get; private set; }
    public float BarPosition   { get; private set; }
    public float BarSize       { get; private set; }
    public float CatchProgress { get; private set; }
    public bool  IsActive      { get; private set; }

    // -------------------------------------------------------------------------
    // Physics constants
    // -------------------------------------------------------------------------
    private const float BarRiseSpeed = 1.2f;
    private const float BarFallSpeed = 0.7f;

    // -------------------------------------------------------------------------
    // Session parameters (derived from difficulty)
    // -------------------------------------------------------------------------
    private readonly float _fishSpeed;
    private readonly float _fishChaos;
    private readonly float _fillRate;
    private readonly float _drainRate;
    private readonly float _maxDuration;
    private readonly FishData _fishData;

    // -------------------------------------------------------------------------
    // Runtime state
    // -------------------------------------------------------------------------
    private float _fishVelocity;
    private float _chaosCooldown;
    private float _elapsed;

    // -------------------------------------------------------------------------
    // Callbacks (Actions instead of events to avoid external subscription leaks)
    // -------------------------------------------------------------------------
    private readonly Action<FishData> _onCaught;
    private readonly Action           _onEscaped;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------
    public SkillCheck(FishData fishData, Action<FishData> onCaught, Action onEscaped)
    {
        _fishData  = fishData;
        _onCaught  = onCaught;
        _onEscaped = onEscaped;

        float d = fishData != null ? Mathf.Clamp01(fishData.catchDifficulty) : 0.3f;

        BarSize      = Mathf.Lerp(0.50f, 0.25f, d); // Barra más grande
        _fishSpeed   = Mathf.Lerp(0.20f, 0.80f, d); // Pez un poco más lento
        _fishChaos   = Mathf.Lerp(1.00f, 4.00f, d);
        _fillRate    = Mathf.Lerp(0.60f, 0.40f, d); // Se llena más rápido
        _drainRate   = Mathf.Lerp(0.20f, 0.40f, d); // Se vacía más lento
        _maxDuration = 12f;

        FishPosition   = 0.5f;
        BarPosition    = Mathf.Clamp(0.5f - BarSize * 0.5f, 0f, 1f - BarSize);
        CatchProgress  = 0f;
        _elapsed       = 0f;
        _fishVelocity  = _fishSpeed * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
        _chaosCooldown = 1f / _fishChaos;
        IsActive       = true;
    }

    // -------------------------------------------------------------------------
    // Main update — called by PlayerController every frame
    // -------------------------------------------------------------------------
    public void Tick(float deltaTime, bool isKeyHeld)
    {
        if (!IsActive) return;

        _elapsed += deltaTime;

        TickFish(deltaTime);
        TickBar(deltaTime, isKeyHeld);
        TickProgress(deltaTime);

        if (CatchProgress >= 1f)
        {
            IsActive = false;
            _onCaught?.Invoke(_fishData);
            return;
        }

        if (_elapsed >= _maxDuration)
        {
            IsActive = false;
            _onEscaped?.Invoke();
        }
    }

    // -------------------------------------------------------------------------
    // Private tick helpers
    // -------------------------------------------------------------------------
    private void TickFish(float dt)
    {
        _chaosCooldown -= dt;

        if (_chaosCooldown <= 0f)
        {
            _fishVelocity  = _fishSpeed * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
            _chaosCooldown = 1f / _fishChaos;
        }

        FishPosition += _fishVelocity * dt;

        if (FishPosition <= 0f)
        {
            FishPosition  = 0f;
            _fishVelocity = Mathf.Abs(_fishVelocity);
        }
        else if (FishPosition >= 1f)
        {
            FishPosition  = 1f;
            _fishVelocity = -Mathf.Abs(_fishVelocity);
        }
    }

    private void TickBar(float dt, bool isKeyHeld)
    {
        BarPosition += (isKeyHeld ? BarRiseSpeed : -BarFallSpeed) * dt;
        BarPosition  = Mathf.Clamp(BarPosition, 0f, 1f - BarSize);
    }

    private void TickProgress(float dt)
    {
        bool fishInBar = FishPosition >= BarPosition && FishPosition <= BarPosition + BarSize;
        CatchProgress += (fishInBar ? _fillRate : -_drainRate) * dt;
        CatchProgress  = Mathf.Clamp01(CatchProgress);
    }
}
