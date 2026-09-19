using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_WikiMenu : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform _contentContainer; // El layout group donde van los slots
    [SerializeField] private GameObject _fishSlotPrefab; // Prefab de UI_WikiSlot

    private bool _isInitialized = false;

    private void OnEnable()
    {
        if (!_isInitialized)
        {
            PopulateWiki();
            _isInitialized = true;
        }
    }

    private void PopulateWiki()
    {
        // Limpiar dummy objects
        foreach (Transform child in _contentContainer)
        {
            Destroy(child.gameObject);
        }

        FishData[] allFishes = Resources.LoadAll<FishData>("Fish");

        foreach (FishData fish in allFishes)
        {
            // Solo peces orgánicos (basura tiene valor negativo)
            if (fish.valuePerKg <= 0) continue;

            GameObject slotObj = Instantiate(_fishSlotPrefab, _contentContainer);
            if (slotObj.TryGetComponent(out UI_WikiSlot slot))
            {
                slot.Setup(fish);
            }
        }
    }

    public void CloseGallery()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI_Cancel");
        gameObject.SetActive(false);
    }
}
