using System.Collections;
using TMPro;
using UnityEngine;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private float _floatSpeed = 1.5f;
    [SerializeField] private float _lifetime   = 0.8f;

    public void Initialize(int delta, Color color)
    {
        _label.text  = $"+{delta}";
        _label.color = color;
        StartCoroutine(AnimateAndDestroy());
    }

    private IEnumerator AnimateAndDestroy()
    {
        float elapsed    = 0f;
        Color startColor = _label.color;

        while (elapsed < _lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _lifetime;

            transform.position += Vector3.up * _floatSpeed * Time.deltaTime;
            startColor.a        = 1f - t;
            _label.color        = startColor;

            yield return null;
        }

        Destroy(gameObject);
    }
}
