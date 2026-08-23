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
        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        
        // Usamos el color base brillante y evitamos tags experimentales que puedan romper el texto
        _label.text  = $"<color=#{colorHex}>+{delta}</color>";
        
        StartCoroutine(AnimateAndDestroy());
    }

    private System.Collections.IEnumerator AnimateAndDestroy()
    {
        float elapsed = 0f;

        while (elapsed < _lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _lifetime;

            // Movimiento
            transform.position += Vector3.up * _floatSpeed * Time.deltaTime;

            // Fade out
            _label.alpha = 1f - t;

            yield return null;
        }

        Destroy(gameObject);
    }
}
