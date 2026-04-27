using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MugController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float rollSpeed = 8f;
    public float maxSpeed  = 8f;

    [Header("Grow Animation")]
    public float growAmount   = 0.05f;
    public float growDuration = 0.4f;

    [Header("Orders Boost")]
    public float ordersBoostSpeed    = 4f;
    public float ordersBoostControl  = 0.15f;
    public float ordersBoostDuration = 5f;

    // ── Private state ─────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isSlippery = false;
    private bool isGrowing  = false;
    private Vector3 targetScale;

    // Orders effect
    private bool  ordersActive           = false;
    private float ordersSpeedMultiplier   = 1f;
    private float ordersControlMultiplier = 1f;
    private float ordersTimer            = 0f;

    // Ice effect
    private bool  iceActive           = false;
    private float iceControlMultiplier = 1f;
    private float iceTimer            = 0f;

    private SpriteRenderer[] mugRenderers;
    private Color[]          originalMugColors;

    // ─────────────────────────────────────────────────────────────────────

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        mugRenderers      = GetComponentsInChildren<SpriteRenderer>();
        originalMugColors = new Color[mugRenderers.Length];
        for (int i = 0; i < mugRenderers.Length; i++)
            originalMugColors[i] = mugRenderers[i].color;

        targetScale = transform.localScale;
        // Apply selected mug sprite
        int selectedMug = PlayerPrefs.GetInt(
            "SelectedMug", 0
        );

        // Load mug sprites from Resources folder
        Sprite[] mugs = Resources.LoadAll<Sprite>(
            "MugSprites"
        );

        if (mugs.Length > selectedMug)
        {
            SpriteRenderer sr = 
                GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.sprite = mugs[selectedMug];
        }
    }

    void Update()
    {
        // ── Tick timers ───────────────────────────────────────────────────
        if (ordersActive)
        {
            ordersTimer -= Time.deltaTime;
            if (ordersTimer <= 0f)
                RemoveOrdersEffect();
        }

        if (iceActive)
        {
            iceTimer -= Time.deltaTime;
            if (iceTimer <= 0f)
                RemoveIceEffect();
        }

        // ── Effective multipliers ─────────────────────────────────────────
        float speedMult   = ordersActive ? ordersSpeedMultiplier   : 1f;
        float controlMult = ordersActive ? ordersControlMultiplier : 1f;
        if (iceActive) controlMult *= iceControlMultiplier;

        float effectiveRollSpeed = rollSpeed * speedMult;
        float effectiveMaxSpeed  = maxSpeed  * speedMult;

        // ── Horizontal movement ───────────────────────────────────────────
        if (!isSlippery)
        {
            float keyboardMove = 0f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) keyboardMove =  1f;
            if (Input.GetKey(KeyCode.LeftArrow)  || Input.GetKey(KeyCode.A)) keyboardMove = -1f;

            rb.linearVelocity = new Vector2(
                keyboardMove * moveSpeed * speedMult,
                rb.linearVelocity.y
            );
        }

        float axisMove = Input.GetAxisRaw("Horizontal");

        if (isGrounded)
            rb.AddForce(new Vector2(axisMove * effectiveRollSpeed * controlMult, 0));
        else
        {
            float airControl = 0.08f * controlMult;
            float targetX    = rb.linearVelocity.x + axisMove * airControl;
            rb.linearVelocity = new Vector2(
                Mathf.Clamp(targetX, -effectiveMaxSpeed, effectiveMaxSpeed),
                rb.linearVelocity.y
            );
        }

        float finalX = Mathf.Clamp(rb.linearVelocity.x, -effectiveMaxSpeed, effectiveMaxSpeed);
        rb.linearVelocity = new Vector2(finalX, rb.linearVelocity.y);

        // ── Jump ──────────────────────────────────────────────────────────
        if ((Input.GetKeyDown(KeyCode.Space) ||
             Input.GetKeyDown(KeyCode.W)     ||
             Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // ── Grow animation ────────────────────────────────────────────────
        if (isGrowing)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime / growDuration
            );
            if (Vector3.Distance(transform.localScale, targetScale) < 0.01f)
            {
                transform.localScale = targetScale;
                isGrowing = false;
            }
        }
    }

    // ── Collision / Trigger ───────────────────────────────────────────────
    // Orders are handled entirely by OrdersPile.cs — nothing here touches Orders.

    void OnCollisionEnter2D(Collision2D col)
    {
        if (IsSlipperyGroundCollision(col))
            isSlippery = true;

        if (IsGroundCollision(col))
            isGrounded = true;

        if (TagMatches(col.gameObject, "Frother"))
            CollectFrother();
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (IsSlipperyGroundCollision(col))
            isSlippery = true;

        if (IsGroundCollision(col))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (IsSlipperyGroundCollision(col))
            isSlippery = false;

        if (IsGroundCollision(col))
            isGrounded = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (TagMatches(other.gameObject, "Frother"))
            CollectFrother();
    }

    void CollectFrother()
    {
        if (FrothManager.Instance != null)
        {
            FrothManager.Instance.FoamCollected();
            OnFoamCollected();
        }
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void OnFoamCollected()
    {
        targetScale += new Vector3(growAmount, growAmount, 0);
        isGrowing = true;
    }

    public void ApplyOrdersEffect(float speedMult, float controlMult, float duration)
    {
        ordersActive            = true;
        ordersSpeedMultiplier   = speedMult;
        ordersControlMultiplier = controlMult;
        ordersTimer             = duration;

        // Immediate lurch so the effect is felt instantly
        float lurchDir = rb.linearVelocity.x >= 0 ? 1f : -1f;
        rb.linearVelocity = new Vector2(lurchDir * moveSpeed * speedMult, rb.linearVelocity.y);

        Debug.Log("Orders chaos ON — speed x" + speedMult + " for " + duration + "s");
    }

    public void RemoveOrdersEffect()
    {
        ordersActive            = false;
        ordersSpeedMultiplier   = 1f;
        ordersControlMultiplier = 1f;
        ordersTimer             = 0f;
        Debug.Log("Orders chaos OFF");
    }

    public void ApplyIceEffect(float duration, float timePenalty)
    {
        iceActive = true;
        iceTimer  = duration;
    }

    public void RemoveIceEffect()
    {
        iceActive = false;
        iceTimer  = 0f;

        if (mugRenderers != null && originalMugColors != null)
            for (int i = 0; i < mugRenderers.Length; i++)
                if (mugRenderers[i] != null)
                    mugRenderers[i].color = originalMugColors[i];
    }

    public void SetSlippery(bool slippery) { isSlippery = slippery; }

    // ── Helpers ───────────────────────────────────────────────────────────

    bool TagMatches(GameObject obj, string tagName)
    {
        while (obj != null)
        {
            if (obj.CompareTag(tagName)) return true;
            obj = obj.transform.parent != null ? obj.transform.parent.gameObject : null;
        }
        return false;
    }

    bool IsSlipperyShelf(GameObject obj)
    {
        return TagMatches(obj, "Slippery Shelf");
    }

    bool IsSlipperyGroundCollision(Collision2D col)
    {
        if (!IsSlipperyShelf(col.gameObject)) return false;
        foreach (ContactPoint2D contact in col.contacts)
            if (contact.normal.y > 0.5f) return true;
        return false;
    }

    bool IsGroundCollision(Collision2D col)
    {
        if (col.gameObject.CompareTag("FallZone")) return false;
        foreach (ContactPoint2D contact in col.contacts)
            if (contact.normal.y > 0.5f) return true;
        return false;
    }

    public void ShowTimePenaltyPopup(Vector3 position, float seconds)
    {
        GameObject popup = new GameObject("TimePenaltyPopup");
        popup.transform.position = position + new Vector3(0, 1f, 0);
        TextMesh text  = popup.AddComponent<TextMesh>();
        text.text      = "-" + seconds + "s!";
        text.fontSize  = 24;
        text.color     = new Color(0.3f, 0.7f, 1f);
        text.alignment = TextAlignment.Center;
        text.anchor    = TextAnchor.MiddleCenter;
        StartCoroutine(AnimatePopup(popup));
    }

    IEnumerator AnimatePopup(GameObject popup)
    {
        float elapsed = 0f; float duration = 1.5f;
        Vector3 startPos = popup.transform.position;
        while (elapsed < duration && popup != null)
        {
            elapsed += Time.deltaTime;
            popup.transform.position = startPos + new Vector3(0, elapsed * 2f, 0);
            TextMesh text = popup.GetComponent<TextMesh>();
            if (text != null) { Color c = text.color; c.a = 1f-(elapsed/duration); text.color = c; }
            yield return null;
        }
        if (popup != null) Destroy(popup);
    }
}