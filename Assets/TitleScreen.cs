using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class TitleScreen : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI pressAnyKeyText;

    [Header("Settings")]
    public string nextSceneName = "PremiseScene";
    public float pulseSpeed = 1.5f;

    private bool canPress = false;

    void Start()
    {
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        // Fade in title first
        if (titleText != null)
        {
            yield return StartCoroutine(
                FadeInText(titleText, 2f)
            );
        }

        yield return new WaitForSeconds(0.5f);

        // Then fade in press any key
        if (pressAnyKeyText != null)
        {
            yield return StartCoroutine(
                FadeInText(pressAnyKeyText, 1f)
            );
            // Start pulsing
            StartCoroutine(
                PulseText(pressAnyKeyText)
            );
        }

        canPress = true;
    }

    void Update()
    {
        if (canPress && Input.anyKeyDown)
        {
            StartCoroutine(LoadTutorial());
        }
    }

    IEnumerator LoadTutorial()
    {
        canPress = false;

        // Fade out everything
        if (titleText != null)
            StartCoroutine(FadeOutText(titleText, 1f));
        if (pressAnyKeyText != null)
            StartCoroutine(
                FadeOutText(pressAnyKeyText, 1f)
            );

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeInText(
        TextMeshProUGUI text, float duration)
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
        c.a = 1f;
        text.color = c;
    }

    IEnumerator FadeOutText(
        TextMeshProUGUI text, float duration)
    {
        float elapsed = 0f;
        Color c = text.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, 
                elapsed / duration);
            text.color = c;
            yield return null;
        }
        c.a = 0f;
        text.color = c;
    }

    IEnumerator PulseText(TextMeshProUGUI text)
    {
        while (true)
        {
            // Fade out
            float elapsed = 0f;
            float duration = 1f / pulseSpeed;
            Color c = text.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0.2f, 
                    elapsed / duration);
                text.color = c;
                yield return null;
            }

            // Fade in
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(0.2f, 1f, 
                    elapsed / duration);
                text.color = c;
                yield return null;
            }
        }
    }
}