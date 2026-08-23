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

    [Header("Skill Check")]
    [Tooltip("Opcional. Si se asigna, se inicializa junto con el slot.")]
    [SerializeField] private SkillCheckPanel _skillCheckPanel;

    public PlayerController Player { get; private set; }

    public void Initialize(PlayerController player)
    {
        Player = player;

        _nameLabel.text  = $"P{player.PlayerID + 1}";
        _scoreLabel.text = "0";

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
            StartCoroutine(PopUpIconRoutine());
        }

        if (_background == null) return;
        Color dimmed = _background.color;
        dimmed.a = 0.12f;
        _background.color = dimmed;
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
}
