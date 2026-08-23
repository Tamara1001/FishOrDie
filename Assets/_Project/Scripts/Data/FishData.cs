using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fish or Die/Fish Data", order = 0)]
public class FishData : ScriptableObject
{
    [Header("Identity")]
    public string fishName = "Unnamed Fish";
    public Sprite fishSprite;

    [Header("Gameplay")]
    [Min(1)]
    public int scoreValue = 10;

    [Range(0f, 1f)]
    [Tooltip("0 = trivial, 1 = nearly impossible. Drives the Skill Check parameters.")]
    public float catchDifficulty = 0.5f;

    [Header("Flavor")]
    [Min(0.1f)]
    public float weight = 1f;
}
