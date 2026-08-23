using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Applies a continuous procedural idle animation (floating up and down) and a 
/// hover scale effect to a UI element (like a game logo) in the Canvas.
/// Uses unscaled time so animations run even when the game is paused.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIFloatingLogo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Floating Animation")]
    [Tooltip("How far up and down the logo floats in pixels.")]
    [SerializeField] private float floatAmplitude = 10f;
    
    [Tooltip("How fast the logo floats up and down.")]
    [SerializeField] private float floatSpeed = 2f;

    [Header("Hover Interaction")]
    [Tooltip("The scale applied to the logo when hovered.")]
    [SerializeField] private float hoverScale = 1.05f;
    
    [Tooltip("How quickly the scale interpolates towards the target scale.")]
    [SerializeField] private float animationSpeed = 10f;

    private RectTransform _rectTransform;
    private Vector2 _originalPosition;
    private Vector3 _originalScale;
    
    private Vector3 _targetScale;
    private bool _isHovered;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        // Cache original values so we can safely animate offset/multipliers
        _originalPosition = _rectTransform.anchoredPosition;
        _originalScale = _rectTransform.localScale;
        
        _targetScale = _originalScale;
    }

    private void Update()
    {
        // ------------------------------------------------------------------
        // IDLE FLOAT
        // Use a Sine wave to create a smooth, continuous bobbing motion.
        // We add the offset to the cached original position to prevent drift.
        // ------------------------------------------------------------------
        float floatOffset = Mathf.Sin(Time.unscaledTime * floatSpeed) * floatAmplitude;
        _rectTransform.anchoredPosition = _originalPosition + new Vector2(0f, floatOffset);

        // ------------------------------------------------------------------
        // HOVER SCALE
        // Smoothly interpolate the localScale towards the current target scale.
        // ------------------------------------------------------------------
        _rectTransform.localScale = Vector3.Lerp(
            _rectTransform.localScale,
            _targetScale,
            animationSpeed * Time.unscaledDeltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        _targetScale = _originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        _targetScale = _originalScale;
    }
}
