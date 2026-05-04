using UnityEngine;
using System.Collections;

public class OrdersPile : MonoBehaviour
{
    [Header("Orders Settings")]
    public float effectDuration    = 5f;
    public float speedMultiplier   = 4f;
    public float controlMultiplier = 0.15f;
    public bool  destroyOnHit      = false;

    private bool onCooldown = false;

    void Awake()
    {
        // Force every Orders collider to be a trigger so the mug
        // always passes through — no matter how it was set in the Inspector
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (onCooldown) return;

        MugController mc = other.GetComponent<MugController>();
        if (mc == null) return;

        onCooldown = true;
        Debug.Log("[OrdersPile] " + gameObject.name + " triggered chaos!");
        StartCoroutine(OrdersEffect(other.gameObject, mc));
    }

    IEnumerator OrdersEffect(GameObject mug, MugController mc)
    {
        mc.ApplyOrdersEffect(speedMultiplier, controlMultiplier, effectDuration);

        // Flash orange 3 times
        SpriteRenderer[] renderers  = mug.GetComponentsInChildren<SpriteRenderer>();
        Color[]          origColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            origColors[i] = renderers[i].color;

        for (int i = 0; i < 3; i++)
        {
            foreach (var sr in renderers)
                if (sr != null) sr.color = new Color(1f, 0.5f, 0.1f);
            yield return new WaitForSeconds(0.15f);

            for (int j = 0; j < renderers.Length; j++)
                if (renderers[j] != null) renderers[j].color = origColors[j];
            yield return new WaitForSeconds(0.15f);
        }

        ShowOrdersPopup(mug.transform.position);

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
        else
        {
            yield return new WaitForSeconds(1f);
            onCooldown = false;
        }
    }

    void ShowOrdersPopup(Vector3 position)
    {
        GameObject popup = new GameObject("OrdersPopup");
        popup.transform.position = position + new Vector3(0, 1f, 0);

        TextMesh text  = popup.AddComponent<TextMesh>();
        text.text      = "TOO MANY ORDERS!";
        text.fontSize  = 18;
        text.color     = new Color(1f, 0.5f, 0.1f);
        text.alignment = TextAlignment.Center;
        text.anchor    = TextAnchor.MiddleCenter;

        StartCoroutine(AnimatePopup(popup));
    }

    IEnumerator AnimatePopup(GameObject popup)
    {
        float   elapsed  = 0f;
        float   duration = 1.5f;
        Vector3 startPos = popup.transform.position;

        while (elapsed < duration && popup != null)
        {
            elapsed += Time.deltaTime;
            popup.transform.position = startPos + new Vector3(0, elapsed * 2f, 0);

            TextMesh text = popup.GetComponent<TextMesh>();
            if (text != null)
            {
                Color c = text.color;
                c.a = 1f - (elapsed / duration);
                text.color = c;
            }
            yield return null;
        }

        if (popup != null) Destroy(popup);
    }
}