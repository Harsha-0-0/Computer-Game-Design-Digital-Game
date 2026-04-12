using UnityEngine;
using System.Collections;
using TMPro;

public class WinSequence : MonoBehaviour
{
    public GameObject backgroundPanel;

    [Header("Audio")]
    public AudioClip victoryMusic;
    private AudioSource audioSource;

    [Header("Text Elements")]
    public TextMeshProUGUI levelCompleteText;
    public TextMeshProUGUI beanCountText;
    public TextMeshProUGUI nextLevelText;

    [Header("Bean Visual")]
    public GameObject smallBeanIcon;
    public GameObject bigBeanIcon;

    [Header("Timing")]
    public float fadeDuration = 1.5f;
    public float pauseBetween = 0.5f;

    public void Play()
    {
        gameObject.SetActive(true);
        // Play victory music
        if (victoryMusic != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = victoryMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        if (backgroundPanel != null)
            backgroundPanel.SetActive(true);
        // Activate all text objects first then set alpha to 0
        if (levelCompleteText != null)
        {
            levelCompleteText.gameObject.SetActive(true);
            SetAlpha(levelCompleteText, 0f);
        }
        if (beanCountText != null)
        {
            beanCountText.gameObject.SetActive(true);
            SetAlpha(beanCountText, 0f);
        }
        if (nextLevelText != null)
        {
            nextLevelText.gameObject.SetActive(true);
            SetAlpha(nextLevelText, 0f);
        }
        if (smallBeanIcon != null) smallBeanIcon.SetActive(false);
        if (bigBeanIcon != null) bigBeanIcon.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // Step 1 — Show Level Complete text
        if (levelCompleteText != null)
        {
            levelCompleteText.text = "LEVEL 1 COMPLETE!";
            yield return StartCoroutine(FadeIn(levelCompleteText, fadeDuration));
        }

        yield return new WaitForSeconds(pauseBetween);

        // Step 2 — Show bean growing
        if (smallBeanIcon != null)
        {
            yield return StartCoroutine(GrowBean(smallBeanIcon, bigBeanIcon));
        }

        yield return new WaitForSeconds(pauseBetween);

        // Step 3 — Show bean count text
        if (beanCountText != null)
        {
            beanCountText.text = "You collected all 5 coffee beans!\nThe bean is 25% grown!";
            yield return StartCoroutine(FadeIn(beanCountText, fadeDuration));
        }

        yield return new WaitForSeconds(pauseBetween);

        // Step 4 — Show Level 2 text with pulse
        if (nextLevelText != null)
        {
            nextLevelText.text = "LEVEL 2 AWAITS!";
            yield return StartCoroutine(FadeIn(nextLevelText, fadeDuration));
            StartCoroutine(PulseText(nextLevelText));
        }
    }

    IEnumerator GrowBean(GameObject small, GameObject big)
    {
        small.SetActive(true);
        if (big != null) big.SetActive(false);

        float elapsed = 0f;
        float duration = 1.5f;
        Vector3 startScale = Vector3.one * 0.3f;
        Vector3 endScale = Vector3.one * 1.5f;
        small.transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            small.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        small.SetActive(false);
        if (big != null)
        {
            big.SetActive(true);
            big.transform.localScale = Vector3.one;
        }
    }

    IEnumerator PulseText(TextMeshProUGUI text)
    {
        while (true)
        {
            float elapsed = 0f;
            float duration = 0.5f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                text.transform.localScale = Vector3.Lerp(
                    Vector3.one,
                    Vector3.one * 1.2f,
                    elapsed / duration
                );
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                text.transform.localScale = Vector3.Lerp(
                    Vector3.one * 1.2f,
                    Vector3.one,
                    elapsed / duration
                );
                yield return null;
            }
        }
    }

    IEnumerator FadeIn(TextMeshProUGUI text, float duration)
    {
        float elapsed = 0f;
        SetAlpha(text, 0f);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(text, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        SetAlpha(text, 1f);
    }

    void SetAlpha(TextMeshProUGUI text, float alpha)
    {
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }
}