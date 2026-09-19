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
    public float minWeightKg = 1f;
    [Tooltip("Peso máximo en KG")]
    public float maxWeightKg = 5f;

    [Tooltip("Valor en dinero por cada KG de peso")]
    public float valuePerKg = 10f;
}
