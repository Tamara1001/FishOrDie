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

    [Header("Feedback Text")]
    [Tooltip("Texto dinámico que da órdenes al jugador en tiempo real.")]
    [SerializeField] private TMPro.TMP_Text _dynamicFeedbackText;

    private PlayerController _player;

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------
    public void Initialize(PlayerController player)
    {
        _player = player;

        if (_canvasGroup != null)
            _canvasGroup.alpha = 0f;
            
        // Crear el texto de feedback dinámico si no existe
        if (_dynamicFeedbackText == null && _canvasGroup != null)
        {
            GameObject txtObj = new GameObject("DynamicFeedbackText");
            txtObj.transform.SetParent(_canvasGroup.transform, false);
            _dynamicFeedbackText = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
            _dynamicFeedbackText.alignment = TMPro.TextAlignmentOptions.Center;
            _dynamicFeedbackText.fontSize = 28f;
            _dynamicFeedbackText.fontStyle = TMPro.FontStyles.Bold;
            
            RectTransform rt = _dynamicFeedbackText.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 25f); // Aparece arriba de la barra
            
            // Outline para legibilidad
            _dynamicFeedbackText.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
            _dynamicFeedbackText.outlineWidth = 0.2f;
            _dynamicFeedbackText.outlineColor = Color.black;
        }
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

        if (_fishIndicator != null)
        {
            // Forzar anclaje al fondo (bottom)
            _fishIndicator.anchorMin = new Vector2(0.5f, 0f);
            _fishIndicator.anchorMax = new Vector2(0.5f, 0f);
            _fishIndicator.pivot     = new Vector2(0.5f, 0.5f); // Centro del pez
            
            float fishY = sc.FishPosition * trackHeight;
            _fishIndicator.anchoredPosition = new Vector2(0f, fishY);
        }

        if (_barFill != null)
        {
            // Forzar anclaje al fondo (bottom) y pivot en la base
            _barFill.anchorMin = new Vector2(0.5f, 0f);
            _barFill.anchorMax = new Vector2(0.5f, 0f);
            _barFill.pivot     = new Vector2(0.5f, 0f); 
            
            float barY      = sc.BarPosition * trackHeight;
            float barHeight = sc.BarSize * trackHeight;

            _barFill.anchoredPosition = new Vector2(0f, barY);
            _barFill.sizeDelta        = new Vector2(_barFill.sizeDelta.x, barHeight);
        }

        if (_progressBar != null)
            _progressBar.fillAmount = sc.CatchProgress;
            
        // Lógica del instructor visual (Feedback)
        if (_dynamicFeedbackText != null)
        {
            float fishBottom = sc.FishPosition; 
            float barBottom = sc.BarPosition;
            float barTop = sc.BarPosition + sc.BarSize;

            if (fishBottom > barTop)
            {
                // Pez arriba de la barra -> hay que subir la barra (Apretar)
                _dynamicFeedbackText.text = "¡APRETÁ MÁS RÁPIDO!";
                _dynamicFeedbackText.color = new Color(1f, 0.5f, 0f); // Naranja
            }
            else if (fishBottom < barBottom)
            {
                // Pez abajo de la barra -> hay que bajar la barra (Soltar)
                _dynamicFeedbackText.text = "¡SOLTÁ!";
                _dynamicFeedbackText.color = new Color(1f, 0.2f, 0.2f); // Rojo alerta
            }
            else
            {
                _dynamicFeedbackText.text = "¡MANTENÉ EL RITMO!";
                float alpha = Mathf.PingPong(Time.time * 8f, 1f); // Titila rápido
                _dynamicFeedbackText.color = new Color(0f, 1f, 0f, 0.4f + alpha * 0.6f); // Verde titilante
            }
            
            // Animación de pulso
            _dynamicFeedbackText.transform.localScale = Vector3.one * (1f + Mathf.PingPong(Time.time * 5f, 0.15f));
        }
    }

    private void SetPanelVisible(bool visible)
    {
        if (_canvasGroup != null)
            _canvasGroup.alpha = visible ? 1f : 0f;
    }
}
