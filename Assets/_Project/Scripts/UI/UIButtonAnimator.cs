// =============================================================================
//  UIButtonAnimator.cs
//  Project : Prismica
//
//  PURPOSE
//  -------
//  Procedurally animates any UI element (Button, Panel, Image, etc.) in
//  response to pointer events, implementing three Disney Animation Principles:
//
//    • Squash & Stretch  — the element deforms on press and release.
//    • Exaggeration      — hover overshoots the rest scale (hoverScale > 1).
//    • Slow In / Slow Out — Lerp with a speed factor produces an
//                           ease-in/ease-out curve: fast at the start,
//                           decelerating as it approaches the target.
//
//  Also smoothly tints any UnityEngine.UI.Graphic (Image, Text, etc.)
//  component on the same GameObject through matching Color.Lerp transitions.
//
//  USAGE
//  -----
//  1. Drop this component onto any UI GameObject that has a RectTransform.
//  2. No additional setup is required — Awake captures the original scale
//     automatically and the pointer interfaces wire themselves up.
//  3. Tune the three exposed floats in the Inspector to taste.
//
//  COMPATIBILITY
//  -------------
//  Requires the Unity UI package (com.unity.ugui) and EventSystem in the scene.
//  Works with both the legacy Input System and the New Input System (the UI
//  pointer interfaces are driven by the EventSystem, not UnityEngine.Input).
// =============================================================================

using UnityEngine;
 using UnityEngine.UI;           // Graphic base class (Image, Text, TMP_Text …)
using UnityEngine.EventSystems;

/// <summary>
/// Drop-on animator that applies Disney Squash &amp; Stretch, Exaggeration, and
/// Slow In / Slow Out to a UI element via the EventSystem pointer interfaces.
/// Also smoothly tints any <see cref="Graphic"/> component on the same
/// GameObject (Image, Text, etc.) through matching Color.Lerp transitions.
/// If no Graphic is present the colour system is silently skipped.
/// </summary>
[DisallowMultipleComponent]
public class UIButtonAnimator : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    // =========================================================================
    //  INSPECTOR — ANIMATION PARAMETERS
    // =========================================================================

    [Header("Scale Targets")]

    [Tooltip("Scale applied when the cursor enters the element. " +
             "Values above 1 create an 'Exaggeration' pop-out effect.")]
    [SerializeField] private float hoverScale = 1.1f;

    [Tooltip("Scale applied the instant the mouse button is held down. " +
             "Values below 1 create the 'Squash' half of Squash & Stretch.")]
    [SerializeField] private float pressedScale = 0.9f;

    [Header("Timing")]

    [Tooltip("Controls how quickly the scale AND colour interpolate toward their targets. " +
             "Higher values = snappier feel. The Lerp formula produces automatic " +
             "ease-in / ease-out: fast at first, decelerating near the target.")]
    [SerializeField] private float animationSpeed = 15f;

    [Header("Colors")]

    [Tooltip("Resting colour applied when the pointer is not interacting with the element.")]
    [SerializeField] private Color defaultColor = new Color(1.00f, 1.00f, 1.00f, 1f); // #FFFFFF

    [Tooltip("Colour applied when the cursor hovers over the element. " +
             "The warm gold tint signals interactivity.")]
    [SerializeField] private Color hoverColor   = new Color(1.00f, 0.93f, 0.64f, 1f); // #FFECA2

    [Tooltip("Colour applied while the mouse button is held down. " +
             "The grey tint reinforces the 'Squash' press feedback.")]
    [SerializeField] private Color pressedColor  = new Color(0.66f, 0.66f, 0.66f, 1f); // #A8A8A8

    // =========================================================================
    //  PRIVATE STATE
    // =========================================================================

    // The scale the element had when it first entered the scene.
    // All animated scales are expressed relative to this, so the animator
    // works correctly even on elements that are pre-scaled in the hierarchy.
    private Vector3 _originalScale;

    // The scale we are currently animating toward.
    // Pointer events write here; Update() chases this value every frame.
    private Vector3 _targetScale;

    // Tracks whether the cursor is currently inside the element's bounds.
    // Used by OnPointerUp to decide whether to return to hover or rest scale.
    private bool _isHovered;

    // Cached Graphic component (Image, Text, or any Graphic subclass) on this
    // GameObject. Null-safe: if no Graphic exists the colour system is skipped.
    // Retrieved once in Awake() via TryGetComponent to avoid per-frame searches.
    private Graphic _graphic;

    // The colour we are currently animating toward.
    // Pointer events write here; Update() Lerps _graphic.color toward this.
    private Color _targetColor;

    // =========================================================================
    //  UNITY LIFECYCLE
    // =========================================================================

    private void Awake()
    {
        // Snapshot the authored scale once.
        // All scale targets are multiples of this value, so the animator
        // is non-destructive and composable with other scale changes.
        _originalScale = transform.localScale;

        // Start the target at rest — no interpolation on the first frame.
        _targetScale = _originalScale;

        // Cache the Graphic on this GameObject (Image, Text, etc.).
        // TryGetComponent is allocation-free; _graphic stays null if none exists.
        TryGetComponent<Graphic>(out _graphic);

        // Initialise the colour target to the resting default so there is no
        // colour flash on the very first frame the element becomes visible.
        _targetColor = defaultColor;
    }

    private void Update()
    {
        // ------------------------------------------------------------------
        //  SCALE — SLOW IN / SLOW OUT  (Disney Animation Principle)
        //  Vector3.Lerp(a, b, t) with t = speed * deltaTime produces a
        //  framerate-independent exponential approach:
        //    — Large gap → fast movement    (Slow Out of the previous pose)
        //    — Small gap → slow movement    (Slow In to the new pose)
        //  This gives every scale transition a natural, cushioned feel.
        // ------------------------------------------------------------------
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            _targetScale,
            animationSpeed * Time.unscaledDeltaTime
        );

        // ------------------------------------------------------------------
        //  COLOUR — matching Lerp using the same animationSpeed.
        //  Guarded by null-check: elements with no Graphic are unaffected.
        //  Color.Lerp interpolates all four channels (R, G, B, A) uniformly,
        //  producing the same ease-in/ease-out feel as the scale animation.
        // ------------------------------------------------------------------
        if (_graphic != null)
        {
            _graphic.color = Color.Lerp(
                _graphic.color,
                _targetColor,
                animationSpeed * Time.unscaledDeltaTime
            );
        }
    }

    // =========================================================================
    //  POINTER INTERFACE IMPLEMENTATIONS
    // =========================================================================

    /// <summary>
    /// Cursor entered the element's rect.
    /// Exaggeration: scale overshoots rest size to draw attention.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered   = true;
        _targetScale = _originalScale * hoverScale;
        _targetColor = hoverColor;
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Hover");
    }

    /// <summary>
    /// Cursor left the element's rect.
    /// Return to the original scale (Slow In back to rest).
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered   = false;
        _targetScale = _originalScale;
        _targetColor = defaultColor;
    }

    /// <summary>
    /// Mouse button pressed inside the element.
    /// Squash: element shrinks suddenly to convey impact and physicality.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        _targetScale = _originalScale * pressedScale;
        _targetColor = pressedColor;
    }

    /// <summary>
    /// Mouse button released.
    /// Stretch: if the cursor is still inside, pop back to the hover scale
    /// (the rebound "stretch" after the squash). If the cursor left during
    /// the press, settle back to the original scale instead.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        // Decide the release target based on whether the pointer is still
        // inside the element — mirrors Button.onClick behaviour.
        _targetScale = _isHovered
            ? _originalScale * hoverScale   // Still hovering → hover scale (Stretch)
            : _originalScale;               // Cursor left    → rest scale

        _targetColor = _isHovered
            ? hoverColor    // Still hovering → return to warm hover tint
            : defaultColor; // Cursor left    → return to neutral white
    }
}
