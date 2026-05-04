using UnityEngine;

public class FoamCollectible : MonoBehaviour
{
    [Header("Bob Settings")]
    public float bobSpeed  = 2f;
    public float bobHeight = 0.2f;

    [Header("Audio")]
    public AudioClip swirlSound;

    [Header("Collected Visual")]
    public Color greyedColor = new Color(0.55f, 0.55f, 0.55f, 0.45f);  // Grey + transparent

    private Vector3      startPos;
    private AudioSource  audioSource;
    private bool         collected = false;

    // Cached refs
    private SpriteRenderer sr;
    private Collider2D     col;

    void Start()
    {
        startPos    = transform.position;
        audioSource = gameObject.AddComponent<AudioSource>();
        sr          = GetComponent<SpriteRenderer>();
        col         = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (collected) return;  // Stop bobbing once collected

        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleFoamCollision(other.gameObject, other.attachedRigidbody, other.transform.root.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleFoamCollision(collision.gameObject, collision.rigidbody, collision.transform.root.gameObject);
    }

    void HandleFoamCollision(GameObject collidedObject, Rigidbody2D attachedRb, GameObject rootObject)
    {
        if (collected) return;  // Already collected — ignore all contacts

        // Resolve mug object (same logic as your original)
        GameObject mugObject = collidedObject;
        if (!mugObject.CompareTag("Mug") && attachedRb != null)
            mugObject = attachedRb.gameObject;
        if (!mugObject.CompareTag("Mug"))
            mugObject = rootObject;

        if (!mugObject.CompareTag("Mug")) return;

        // ── Mark as collected immediately so re-entry can't double-count ──
        collected = true;

        // ── Play swirl sound ──────────────────────────────────────────────
        if (swirlSound != null)
            AudioSource.PlayClipAtPoint(swirlSound, transform.position);

        // ── Tell LevelManager ─────────────────────────────────────────────
        if (LevelManager.Instance != null)
            LevelManager.Instance.FoamCollected();
        else
            Debug.LogWarning("FoamCollectible: LevelManager.Instance is null.");

        // ── Grey out sprite ───────────────────────────────────────────────
        if (sr != null)
            sr.color = greyedColor;

        // ── Stop bobbing — lock to current Y ─────────────────────────────
        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);

        // ── Disable the collider so mug passes through freely ─────────────
        if (col != null)
            col.enabled = false;

        Debug.Log("FoamCollectible: collected and greyed out — " + gameObject.name);
    }
}