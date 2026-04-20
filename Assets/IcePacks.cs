using UnityEngine;
using System.Collections;
using TMPro;

public class IcePack : MonoBehaviour
{
    [Header("Ice Pack Settings")]
    public float timePenalty = 5f;
    public bool destroyOnHit = false;

    private bool hit = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mug") && !hit)
        {
            hit = true;
            StartCoroutine(IcePackHit(
                other.gameObject
            ));

            if (!destroyOnHit)
                StartCoroutine(HitCooldown());
        }
    }

    IEnumerator HitCooldown()
    {
        yield return new WaitForSeconds(2f);
        hit = false;
    }

    IEnumerator IcePackHit(GameObject mug)
    {
        // Flash mug blue
        SpriteRenderer[] renderers =
            mug.GetComponentsInChildren
                <SpriteRenderer>();

        Color[] originalColors =
            new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;

        // Flash blue 3 times
        for (int i = 0; i < 3; i++)
        {
            foreach (var sr in renderers)
                sr.color = new Color(0.5f, 0.8f, 1f);
            yield return new WaitForSeconds(0.15f);

            for (int j = 0; j < renderers.Length; j++)
                renderers[j].color = originalColors[j];
            yield return new WaitForSeconds(0.15f);
        }

        // Reduce timer
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ReduceTime(
                timePenalty
            );
            Debug.Log("Ice pack! -" + 
                timePenalty + "s!");
        }
        else
        {
            Debug.Log("LevelManager not found!");
        }

        // Show floating penalty text
        ShowPenaltyPopup(mug.transform.position);

        if (destroyOnHit)
            Destroy(gameObject);
    }

    void ShowPenaltyPopup(Vector3 position)
    {
        GameObject popup = 
            new GameObject("TimePenaltyPopup");
        popup.transform.position = position +
            new Vector3(0, 1f, 0);

        TextMesh text = 
            popup.AddComponent<TextMesh>();
        text.text = "-" + timePenalty + "s!";
        text.fontSize = 24;
        text.color = new Color(0.3f, 0.7f, 1f);
        text.alignment = TextAlignment.Center;
        text.anchor = TextAnchor.MiddleCenter;

        StartCoroutine(AnimatePopup(popup));
    }

    IEnumerator AnimatePopup(GameObject popup)
    {
        float elapsed = 0f;
        float duration = 1.5f;
        Vector3 startPos = popup.transform.position;

        while (elapsed < duration && 
            popup != null)
        {
            elapsed += Time.deltaTime;

            popup.transform.position = startPos +
                new Vector3(0, elapsed * 2f, 0);

            TextMesh text =
                popup.GetComponent<TextMesh>();
            if (text != null)
            {
                Color c = text.color;
                c.a = 1f - (elapsed / duration);
                text.color = c;
            }

            yield return null;
        }

        if (popup != null)
            Destroy(popup);
    }
}