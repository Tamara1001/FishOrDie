using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(0)]
public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Public State
    public int PlayerID    { get; private set; }
    public string PlayerName { get; private set; }
    public string BindingPath { get; private set; }
    public int CurrentScore { get; private set; }
    public Color PlayerColor { get; private set; }

    public SkillCheck CurrentSkillCheck { get; private set; }

    [Header("UI & Feedback")]
    [SerializeField] private SpriteRenderer _playerVisualRenderer; // SPRITE DEL JUGADOR (HIJO)
    [SerializeField] private SpriteRenderer _feedbackSpriteRenderer;
    [SerializeField] private SpriteRenderer _feedbackOutlineRenderer; // OUTLINE DEL FEEDBACK
    [SerializeField] private Sprite _caughtSprite;
    [SerializeField] private Sprite _lostSprite;
    [SerializeField] private Transform _popupSpawnPoint;

    public Transform PopupSpawnPoint => _popupSpawnPoint != null ? _popupSpawnPoint : transform;

    // -------------------------------------------------------------------------
    // Eventos
    // -------------------------------------------------------------------------
    public event Action<PlayerController>           OnFishAttempt;
    public event Action<PlayerController, FishData> OnFishCaught;

    // -------------------------------------------------------------------------
    // Input
    // -------------------------------------------------------------------------
    private InputAction _fishAction;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------
    private void OnDestroy()
    {
        _fishAction?.Disable();
        _fishAction?.Dispose();
    }

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------
    public void Initialize(int id, string bindingPath, Color color, bool faceLeft, string playerName)
    {
        PlayerID    = id;
        PlayerName  = playerName;
        BindingPath = bindingPath;
        PlayerColor = color;
        
        if (_playerVisualRenderer != null)
        {
            _playerVisualRenderer.color = color;
            // Invertir el sprite si está a la derecha
            _playerVisualRenderer.flipX = faceLeft;
        }
        else
        {
            Debug.LogWarning($"[PlayerController] {_playerVisualRenderer} no está asignado en {gameObject.name}");
        }

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
            
            // Tensión visual (vibración) mientras pesca
            if (_playerVisualRenderer != null)
            {
                Transform vis = _playerVisualRenderer.transform;
                // Vibración suave de base, vibración fuerte si está presionando/tirando
                float shakeForce = _fishAction.IsPressed() ? 0.06f : 0.015f; 
                vis.localPosition = new Vector3(
                    UnityEngine.Random.Range(-shakeForce, shakeForce),
                    UnityEngine.Random.Range(-shakeForce, shakeForce),
                    0f
                );
            }
            return;
        }
        else
        {
            // Restablecer posición si no está pescando ni en eliminación
            if (_playerVisualRenderer != null && gameObject.activeSelf && _squashRoutine == null)
            {
                _playerVisualRenderer.transform.localPosition = Vector3.Lerp(_playerVisualRenderer.transform.localPosition, Vector3.zero, Time.deltaTime * 10f);
            }
        }

        if (_fishAction.WasPressedThisFrame())
        {
            DoSquashAndStretch();
            OnFishAttempt?.Invoke(this);
        }
    }

    private Vector3 _baseVisualScale = Vector3.one;
    private bool _hasSavedBaseScale = false;

    private Coroutine _squashRoutine;
    private void DoSquashAndStretch()
    {
        if (_playerVisualRenderer == null) return;
        
        if (!_hasSavedBaseScale)
        {
            _baseVisualScale = _playerVisualRenderer.transform.localScale;
            _hasSavedBaseScale = true;
        }

        if (_squashRoutine != null) StopCoroutine(_squashRoutine);
        _squashRoutine = StartCoroutine(SquashRoutine());
    }

    private System.Collections.IEnumerator SquashRoutine()
    {
        Transform vis = _playerVisualRenderer.transform;
        
        float baseScaleX = _baseVisualScale.x;
        float baseScaleY = _baseVisualScale.y;
        float baseScaleZ = _baseVisualScale.z;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Una curva simple que baja a 0.7 y vuelve a 1.0 en Y, y se ensancha en X
            float scaleY = baseScaleY * (1f - Mathf.Sin(t * Mathf.PI) * 0.3f);
            float scaleX = baseScaleX * (1f + Mathf.Sin(t * Mathf.PI) * 0.3f);
            
            vis.localScale = new Vector3(scaleX, scaleY, baseScaleZ);
            yield return null;
        }

        vis.localScale = _baseVisualScale;
        _squashRoutine = null; // Liberamos la variable para que otros efectos puedan funcionar
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
        ShowFeedback(_caughtSprite);
        OnFishCaught?.Invoke(this, fish);
    }

    private void HandleSkillCheckEscaped()
    {
        CurrentSkillCheck = null;
        ShowFeedback(_lostSprite);
        Debug.Log($"[{gameObject.name}] ¡El pez escapó!");
    }

    public void CancelSkillCheck()
    {
        CurrentSkillCheck = null;
    }

    // -------------------------------------------------------------------------
    // Feedback Routine
    // -------------------------------------------------------------------------
    private Coroutine _feedbackRoutine;
    private void ShowFeedback(Sprite sprite)
    {
        if (_feedbackSpriteRenderer == null || sprite == null) return;
        
        if (_feedbackRoutine != null) StopCoroutine(_feedbackRoutine);
        _feedbackRoutine = StartCoroutine(FeedbackRoutine(sprite));
    }

    private System.Collections.IEnumerator FeedbackRoutine(Sprite sprite)
    {
        _feedbackSpriteRenderer.sprite = sprite;
        _feedbackSpriteRenderer.gameObject.SetActive(true);
        
        Color targetColor = (sprite == _caughtSprite) ? Color.green : Color.red;

        if (_feedbackOutlineRenderer != null)
        {
            _feedbackOutlineRenderer.sprite = sprite;
            _feedbackOutlineRenderer.gameObject.SetActive(true);
        }
        
        float duration = 1.2f;
        float elapsed = 0f;
        Vector3 startPos = Vector3.up * 0.8f; // Arriba del jugador
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Sube suavemente
            Vector3 currentPos = startPos + Vector3.up * (t * 1f);
            _feedbackSpriteRenderer.transform.localPosition = currentPos;
            
            if (_feedbackOutlineRenderer != null)
                _feedbackOutlineRenderer.transform.localPosition = currentPos;
            
            // Fade out en la segunda mitad
            float alpha = 1f;
            if (t > 0.5f) alpha = 1f - ((t - 0.5f) * 2f);
            
            // Aplicamos los colores intercambiados
            Color mainColor = PlayerColor;
            mainColor.a = alpha;
            _feedbackSpriteRenderer.color = mainColor;

            if (_feedbackOutlineRenderer != null)
            {
                targetColor.a = alpha;
                _feedbackOutlineRenderer.color = targetColor;
            }
            
            yield return null;
        }
        
        _feedbackSpriteRenderer.gameObject.SetActive(false);
        if (_feedbackOutlineRenderer != null) _feedbackOutlineRenderer.gameObject.SetActive(false);
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
        
        StartCoroutine(DragDownRoutine());
    }

    private System.Collections.IEnumerator DragDownRoutine()
    {
        if (_playerVisualRenderer == null) yield break;
        
        Transform vis = _playerVisualRenderer.transform;
        Vector3 startPos = vis.localPosition;
        Vector3 targetPos = startPos + Vector3.down * 3f; // Jalado hacia abajo
        
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Curva acelerada (ease in) para dar sensación de fuerza
            float easeIn = t * t * t;
            
            vis.localPosition = Vector3.Lerp(startPos, targetPos, easeIn);
            
            // Giro violento
            vis.Rotate(Vector3.forward, 720f * Time.deltaTime);

            // Escalar un poquito hacia cero simulando perspectiva
            vis.localScale = Vector3.Lerp(vis.localScale, Vector3.zero, easeIn * 0.1f);

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
