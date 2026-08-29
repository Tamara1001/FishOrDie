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
    public bool IsFacingLeft { get; private set; }

    public SkillCheck CurrentSkillCheck { get; private set; }

    [Header("UI & Feedback")]
    [Tooltip("El objeto PADRE que contiene todas las partes visuales del jugador (bote, brazos, cabeza).")]
    [SerializeField] private Transform _playerVisualRoot; 
    [SerializeField] private SpriteRenderer _feedbackSpriteRenderer;
    [SerializeField] private SpriteRenderer _feedbackOutlineRenderer; // OUTLINE DEL FEEDBACK
    [SerializeField] private Sprite _caughtSprite;
    [SerializeField] private Sprite _lostSprite;
    [SerializeField] private Transform _popupSpawnPoint;
    
    [Header("Boya / Línea de Pesca")]
    [Tooltip("El visual principal de la boya (para hacerla temblar durante la pesca)")]
    [SerializeField] private Transform _bobberVisual;
    [Tooltip("Objeto visual que salta o aparece cuando el pez muerde (Ej: signo !)")]
    [SerializeField] private GameObject _bobberAlertObject;

    [Header("Posicionamiento Relativo (Espejado)")]
    [Tooltip("Objetos que deben moverse al lado opuesto cuando el jugador mira a la izquierda (Ej: Hilo, Boya, Alerta, Canvas del SkillCheck). Esto invierte su Posición X local sin rotarlos ni deformarlos.")]
    [SerializeField] private Transform[] _flipOffsets;

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
    private AudioSource _localAudioSource;
    private bool _isPlayingTensionSFX;

    public void Initialize(int id, string bindingPath, Color color, bool faceLeft, string playerName)
    {
        PlayerID    = id;
        PlayerName  = playerName;
        BindingPath = bindingPath;
        PlayerColor = color;
        IsFacingLeft = faceLeft;
        
        // Crear un AudioSource local dinámicamente para ruidos que se deben cortar (Ej: la caña)
        _localAudioSource = gameObject.AddComponent<AudioSource>();
        _localAudioSource.loop = true;
        _localAudioSource.playOnAwake = false;
        
        if (AudioManager.Instance != null)
        {
            _localAudioSource.outputAudioMixerGroup = AudioManager.Instance.SFXMixerGroup;
            if (AudioManager.Instance.TryGetSFXEntry("SkillCheck_Tension", out var entry))
            {
                _localAudioSource.clip = entry.clip;
                _localAudioSource.volume = entry.volume > 0 ? entry.volume : 1f;
            }
        }
        
        if (_playerVisualRoot != null)
        {
            // Teñir todos los SpriteRenderers hijos (cabeza, bote, etc)
            SpriteRenderer[] childSprites = _playerVisualRoot.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in childSprites)
            {
                sr.color = color;
            }

            // Invertir el objeto padre entero si mira hacia la izquierda
            Vector3 scale = _playerVisualRoot.localScale;
            scale.x = faceLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            _playerVisualRoot.localScale = scale;
        }
        else
        {
            Debug.LogWarning($"[PlayerController] _playerVisualRoot no está asignado en {gameObject.name}");
        }

        // Si el jugador debe mirar a la izquierda, invertimos la Posición X de los objetos asimétricos
        if (faceLeft && _flipOffsets != null)
        {
            foreach (Transform t in _flipOffsets)
            {
                if (t != null)
                {
                    Vector3 localPos = t.localPosition;
                    localPos.x = -localPos.x;
                    t.localPosition = localPos;
                }
            }
        }

        if (_bobberVisual != null)
        {
            _baseBobberPos = _bobberVisual.localPosition;
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
            
            // Audio del riel de pesca (solo suena cuando está interactuando)
            if (_fishAction.IsPressed())
            {
                if (!_isPlayingTensionSFX && _localAudioSource != null && _localAudioSource.clip != null)
                {
                    _localAudioSource.Play();
                    _isPlayingTensionSFX = true;
                }
            }
            else
            {
                if (_isPlayingTensionSFX && _localAudioSource != null)
                {
                    _localAudioSource.Stop();
                    _isPlayingTensionSFX = false;
                }
            }

            // Tensión visual (vibración) mientras pesca
            if (_playerVisualRoot != null)
            {
                // Vibración suave de base, vibración fuerte si está presionando/tirando
                float shakeForce = _fishAction.IsPressed() ? 0.06f : 0.015f; 
                _playerVisualRoot.localPosition = new Vector3(
                    UnityEngine.Random.Range(-shakeForce, shakeForce),
                    UnityEngine.Random.Range(-shakeForce, shakeForce),
                    0f
                );
            }
            
            // Hacer vibrar a la boya de forma similar (o más frenética)
            if (_bobberVisual != null)
            {
                float shakeForce = _fishAction.IsPressed() ? 0.08f : 0.02f;
                _bobberVisual.localPosition = _baseBobberPos + new Vector3(
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
            if (_playerVisualRoot != null && gameObject.activeSelf && _squashRoutine == null)
            {
                _playerVisualRoot.localPosition = Vector3.Lerp(_playerVisualRoot.localPosition, Vector3.zero, Time.deltaTime * 10f);
            }
            
            if (_bobberVisual != null && gameObject.activeSelf)
            {
                _bobberVisual.localPosition = Vector3.Lerp(_bobberVisual.localPosition, _baseBobberPos, Time.deltaTime * 10f);
            }
        }

        if (_fishAction.WasPressedThisFrame())
        {
            DoSquashAndStretch();
            OnFishAttempt?.Invoke(this);
        }
    }

    private Vector3 _baseVisualScale = Vector3.one;
    private Vector3 _baseBobberPos = Vector3.zero;
    private bool _hasSavedBaseScale = false;

    private Coroutine _squashRoutine;
    private void DoSquashAndStretch()
    {
        if (_playerVisualRoot == null) return;
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Player_InputPress");

        if (!_hasSavedBaseScale)
        {
            _baseVisualScale = _playerVisualRoot.localScale;
            _hasSavedBaseScale = true;
        }

        if (_squashRoutine != null) StopCoroutine(_squashRoutine);
        _squashRoutine = StartCoroutine(SquashRoutine());
    }

    private System.Collections.IEnumerator SquashRoutine()
    {
        Transform vis = _playerVisualRoot;
        
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
        StopLocalTensionAudio();
        ShowFeedback(_caughtSprite);
        
        // Un solo sonido de captura, pero variamos el pitch dinámicamente según la dificultad.
        // Pez fácil (Mojarra, diff ~0.2) -> Pitch alto (1.2f, suena a pescadito chiquito)
        // Pez difícil (Dorado, diff ~0.9) -> Pitch bajo (0.8f, suena a pez gordo y pesado)
        if (AudioManager.Instance != null)
        {
            float dynamicPitch = Mathf.Lerp(1.2f, 0.8f, fish.catchDifficulty);
            AudioManager.Instance.PlaySFX("Caught_Fish", dynamicPitch);
        }

        OnFishCaught?.Invoke(this, fish);
    }

    private void HandleSkillCheckEscaped()
    {
        CurrentSkillCheck = null;
        StopLocalTensionAudio();
        ShowFeedback(_lostSprite);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Fish_Escaped");
        Debug.Log($"[{gameObject.name}] ¡El pez escapó!");
    }

    public void CancelSkillCheck()
    {
        CurrentSkillCheck = null;
        StopLocalTensionAudio();
    }

    private void StopLocalTensionAudio()
    {
        if (_isPlayingTensionSFX && _localAudioSource != null)
        {
            _localAudioSource.Stop();
            _isPlayingTensionSFX = false;
        }
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
        StopLocalTensionAudio();
        _fishAction?.Disable();
        Debug.Log($"[{gameObject.name}] ¡Devorado por el Monstruo del Paraná!");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Monster_Drag");
        }
        
        StartCoroutine(EliminateRoutine());
    }

    private System.Collections.IEnumerator EliminateRoutine()
    {
        if (_playerVisualRoot == null) yield break;

        Transform vis = _playerVisualRoot;
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

    // -------------------------------------------------------------------------
    // Lógica de la Boya
    // -------------------------------------------------------------------------
    public void TriggerBobberAlert()
    {
        if (_bobberAlertObject != null && _bobberAlertObject.activeSelf == false)
        {
            StartCoroutine(BobberAlertRoutine());
        }
    }

    private System.Collections.IEnumerator BobberAlertRoutine()
    {
        _bobberAlertObject.SetActive(true);
        Vector3 origScale = _bobberAlertObject.transform.localScale;
        
        // Efecto "Pop"
        _bobberAlertObject.transform.localScale = origScale * 1.5f;
        
        float t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            _bobberAlertObject.transform.localScale = Vector3.Lerp(origScale * 1.5f, origScale, t / 0.15f);
            yield return null;
        }
        
        _bobberAlertObject.transform.localScale = origScale;
        
        // El pez se queda pausado 0.5s en total, así que mostramos el signo ! un poquito y lo apagamos
        yield return new WaitForSeconds(0.35f); 
        
        _bobberAlertObject.SetActive(false);
    }
}
