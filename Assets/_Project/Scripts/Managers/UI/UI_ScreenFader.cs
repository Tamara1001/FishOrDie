using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton simple para manejar un panel negro de transición.
/// </summary>
public class UI_ScreenFader : MonoBehaviour
{
    public static UI_ScreenFader Instance { get; private set; }

    [Tooltip("El CanvasGroup asociado a la imagen negra que tapa la pantalla")]
    public CanvasGroup fadeGroup;

    private void Awake()
    {
        Instance = this;
        // Arrancar con la pantalla en negro y hacer un fade in automático al cargar la escena
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f;
            fadeGroup.blocksRaycasts = true;
            FadeTo(0f, 1f); // Tarda 1 segundo en revelar la pantalla
        }
    }

    public void FadeTo(float targetAlpha, float duration)
    {
        if (fadeGroup == null) return;

        // Bloqueamos clics si estamos oscureciendo la pantalla
        fadeGroup.blocksRaycasts = targetAlpha > 0f;

        StopAllCoroutines();
        StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = fadeGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
    }
}