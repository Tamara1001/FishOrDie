using UnityEngine;
using UnityEngine.UI;

public class SkillCheckPanel : MonoBehaviour
{
    [Header("Track Layout")]
    [Tooltip("RectTransform que define el área total del track (toda la altura posible).")]
    [SerializeField] private RectTransform _trackArea;

    [Tooltip("Ícono del pez. Se ancla en el centro del track y se desplaza en Y.")]
    [SerializeField] private RectTransform _fishIndicator;

    [Tooltip("Zona verde que el jugador controla. Su altura y posición Y reflejan BarSize y BarPosition.")]
    [SerializeField] private RectTransform _barFill;

    [Header("Progress")]
    [Tooltip("Image con Fill Method = Vertical y Fill Origin = Bottom. FillAmount = CatchProgress.")]
    [SerializeField] private Image _progressBar;

    [Header("Panel")]
    [Tooltip("CanvasGroup raíz del panel. Alpha = 0 cuando el Skill Check no está activo.")]
    [SerializeField] private CanvasGroup _canvasGroup;

    private PlayerController _player;

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------
    public void Initialize(PlayerController player)
    {
        _player = player;

        if (_canvasGroup != null)
            _canvasGroup.alpha = 0f;
    }

    // -------------------------------------------------------------------------
    // Visual Update (polling — no event subscriptions)
    // -------------------------------------------------------------------------
    private void Update()
    {
        if (_player == null) return;

        SkillCheck sc = _player.CurrentSkillCheck;

        if (sc == null || !sc.IsActive)
        {
            SetPanelVisible(false);
            return;
        }

        SetPanelVisible(true);
        ApplyVisuals(sc);
    }

    // -------------------------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------------------------
    private void ApplyVisuals(SkillCheck sc)
    {
        if (_trackArea == null) return;

        float trackHeight = _trackArea.rect.height;
        float halfTrack   = trackHeight * 0.5f;

        if (_fishIndicator != null)
        {
            float fishY = sc.FishPosition * trackHeight - halfTrack;
            _fishIndicator.anchoredPosition = new Vector2(0f, fishY);
        }

        if (_barFill != null)
        {
            float barY      = sc.BarPosition * trackHeight - halfTrack;
            float barHeight = sc.BarSize * trackHeight;

            _barFill.anchoredPosition = new Vector2(0f, barY);
            _barFill.sizeDelta        = new Vector2(_barFill.sizeDelta.x, barHeight);
        }

        if (_progressBar != null)
            _progressBar.fillAmount = sc.CatchProgress;
    }

    private void SetPanelVisible(bool visible)
    {
        if (_canvasGroup != null)
            _canvasGroup.alpha = visible ? 1f : 0f;
    }
}
