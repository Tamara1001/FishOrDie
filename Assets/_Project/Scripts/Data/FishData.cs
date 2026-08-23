using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fish or Die/Fish Data", order = 0)]
public class FishData : ScriptableObject
{
    [Header("Identity")]
    public string fishName = "Pez Genérico";
    public Sprite fishSprite;

    [Header("Gameplay (Skill Check)")]
    [Range(0f, 1f)]
    [Tooltip("0 = muy fácil, 1 = casi imposible")]
    public float catchDifficulty = 0.5f;

    [Header("Fish Stats (Generación)")]
    [Tooltip("Tamaño mínimo en CM")]
    public int minSizeCm = 15;
    [Tooltip("Tamaño máximo en CM")]
    public int maxSizeCm = 50;

    [Tooltip("Peso mínimo en KG")]
    public int minWeightKg = 1;
    [Tooltip("Peso máximo en KG")]
    public int maxWeightKg = 5;

    [Tooltip("Precio (Score) por Kg de este pez")]
    public int valuePerKg = 10;
}
