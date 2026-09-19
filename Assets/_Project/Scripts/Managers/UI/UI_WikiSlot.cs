using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_WikiSlot : MonoBehaviour
{
    [SerializeField] private Image _fishImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _sizeText;
    [SerializeField] private TextMeshProUGUI _weightText;
    [SerializeField] private TextMeshProUGUI _valueText;

    public void Setup(FishData data)
    {
        if (_fishImage != null) _fishImage.sprite = data.fishSprite;
        if (_nameText != null) _nameText.text = data.fishName;
        if (_sizeText != null) _sizeText.text = $"{data.minSizeCm}cm - {data.maxSizeCm}cm";

        if (_weightText != null)
        {
            string minW = data.minWeightKg < 1f ? $"{Mathf.RoundToInt(data.minWeightKg * 1000f)}g" : $"{data.minWeightKg:F1}kg";
            string maxW = data.maxWeightKg < 1f ? $"{Mathf.RoundToInt(data.maxWeightKg * 1000f)}g" : $"{data.maxWeightKg:F1}kg";
            _weightText.text = $"{minW} - {maxW}";
        }

        if (_valueText != null) _valueText.text = $"${Mathf.RoundToInt(data.valuePerKg)} x Kg";
    }
}
