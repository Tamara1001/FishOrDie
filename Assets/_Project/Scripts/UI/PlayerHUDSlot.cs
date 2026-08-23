using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _nameLabel;
    [SerializeField] private TMP_Text _scoreLabel;
    [SerializeField] private Image _background;
    [SerializeField] private GameObject _eliminatedIcon;
    [SerializeField] private TMP_Text _catchBannerText;
    [SerializeField] private CanvasGroup _catchBannerGroup;

    [Header("Skill Check")]
    [Tooltip("Opcional. Si se asigna, se inicializa junto con el slot.")]
    [SerializeField] private SkillCheckPanel _skillCheckPanel;

    public PlayerController Player { get; private set; }

    public void Initialize(PlayerController player)
    {
        Player = player;

        // Ej: "<Keyboard>/a" -> "A"
        string cleanKey = UnityEngine.InputSystem.InputControlPath.ToHumanReadableString(
            player.BindingPath, UnityEngine.InputSystem.InputControlPath.HumanReadableStringOptions.OmitDevice);

        _nameLabel.text  = $"{player.PlayerName} [{cleanKey.ToUpper()}]";
        _scoreLabel.text = "0";

        if (_nameLabel != null) { Color c = _nameLabel.color; c.a = 1f; _nameLabel.color = c; }
        if (_scoreLabel != null) { Color c = _scoreLabel.color; c.a = 1f; _scoreLabel.color = c; }

        if (_background != null)
        {
            Color tint = player.PlayerColor;
            tint.a = 0.4f;
            _background.color = tint;
        }

        _eliminatedIcon?.SetActive(false);

        _skillCheckPanel?.Initialize(player);
    }

    public void RefreshScore()
    {
        if (Player != null)
            _scoreLabel.text = Player.CurrentScore.ToString();
    }

    public void MarkEliminated()
    {
        if (_eliminatedIcon != null)
        {
            _eliminatedIcon.SetActive(true);
            if (_eliminatedIcon.TryGetComponent(out Image skullImg))
            {
                Color c = skullImg.color;
                c.a = 1f; // Forzar que la calavera sea 100% visible
                skullImg.color = c;
            }
            StartCoroutine(PopUpIconRoutine());
        }

        // Bajar opacidad del fondo
        if (_background != null)
        {
            Color dimmed = _background.color;
            dimmed.a = 0.1f; // Casi transparente
            _background.color = dimmed;
        }

        // Bajar opacidad de los textos al 30%
        if (_nameLabel != null) { Color c = _nameLabel.color; c.a = 0.3f; _nameLabel.color = c; }
        if (_scoreLabel != null) { Color c = _scoreLabel.color; c.a = 0.3f; _scoreLabel.color = c; }
    }

    private System.Collections.IEnumerator PopUpIconRoutine()
    {
        Transform iconTransform = _eliminatedIcon.transform;
        iconTransform.localScale = Vector3.zero;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Easing Out Back (overshoot) custom curve
            float ease = 1f + 2.70158f * Mathf.Pow(t - 1f, 3) + 1.70158f * Mathf.Pow(t - 1f, 2);
            
            iconTransform.localScale = Vector3.one * Mathf.Max(0f, ease);
            yield return null;
        }

        iconTransform.localScale = Vector3.one;
    }

    private Coroutine _catchBannerRoutine;
    
    public void ShowCatchBanner(string fishName, int size, int weight, int score)
    {
        if (_catchBannerText == null) return;
        
        if (_catchBannerRoutine != null) StopCoroutine(_catchBannerRoutine);
        _catchBannerRoutine = StartCoroutine(CatchBannerRoutine(fishName, size, weight, score));
    }

    private System.Collections.IEnumerator CatchBannerRoutine(string fishName, int size, int weight, int score)
    {
        _catchBannerText.text = $"{fishName}\n{size}cm - {weight}kg\n<color=#FFD700>+${score}</color>";
        
        // Manejamos el fade usando CanvasGroup si está asignado, sino solo el texto
        if (_catchBannerGroup != null)
        {
            _catchBannerGroup.gameObject.SetActive(true);
            _catchBannerGroup.alpha = 1f;
        }
        else
        {
            _catchBannerText.gameObject.SetActive(true);
            Color baseColor = _catchBannerText.color;
            baseColor.a = 1f;
            _catchBannerText.color = baseColor;
        }
        
        float duration = 5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            if (elapsed > 4f)
            {
                float alpha = 1f - (elapsed - 4f);
                if (_catchBannerGroup != null)
                {
                    _catchBannerGroup.alpha = alpha;
                }
                else
                {
                    Color c = _catchBannerText.color;
                    c.a = alpha;
                    _catchBannerText.color = c;
                }
            }
            
            yield return null;
        }
        
        if (_catchBannerGroup != null) _catchBannerGroup.gameObject.SetActive(false);
        else _catchBannerText.gameObject.SetActive(false);
    }
}
