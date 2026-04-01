using UnityEngine;
using System.Collections;
using TMPro;

public class WinSequence : MonoBehaviour
{
    public TextMeshProUGUI line1;

    public void Play()
    {
        StartCoroutine(ShowLines());
    }

    IEnumerator ShowLines()
    {
        yield return StartCoroutine(FadeIn(line1, 1.5f));
    }

    IEnumerator FadeIn(TextMeshProUGUI text, float duration)
    {
        float elapsed = 0f;
        Color c = text.color;
        c.a = 0f;
        text.color = c;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            text.color = c;
            yield return null;
        }
    }
}