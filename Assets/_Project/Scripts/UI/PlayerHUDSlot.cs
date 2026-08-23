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
        _eliminatedIcon?.SetActive(true);

        if (_background == null) return;
        Color dimmed = _background.color;
        dimmed.a = 0.12f;
        _background.color = dimmed;
    }
}
