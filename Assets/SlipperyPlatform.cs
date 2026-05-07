using UnityEngine;

public class SlipperyPlatform : MonoBehaviour
{
    [Header("Slippery Settings")]
    public float slideForce = 3f;
    public float controlForce = 4f;
    public float maxSlideSpeed = 6f;
    public float slideDirection = 1f;
    // 1 = slides right, -1 = slides left

    private bool mugOnPlatform = false;
    private Rigidbody2D mugRb;
    private MugController mugController;

    // ── Level 2 milk spill (only active when LevelManager_2 exists) ──────────
    private LevelManager_2 levelManager2;
    private float slipperyTimer = 0f;
    private bool level2ModeActive = false;
    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        // Check once at scene load — if LevelManager_2 exists, we're in Level 2
        levelManager2 = FindObjectOfType<LevelManager_2>();
        level2ModeActive = levelManager2 != null;
    }

    void FixedUpdate()
    {
        if (!mugOnPlatform || mugRb == null) return;

        float input = Input.GetAxis("Horizontal");

        // Always push mug in slide direction
        // even when standing still
        mugRb.AddForce(
            new Vector2(slideForce * slideDirection, 0f)
        );

        // Player can push back but with less force
        if (Mathf.Abs(input) > 0.1f)
        {
            mugRb.AddForce(
                new Vector2(input * controlForce, 0f)
            );
        }

        // Clamp max speed
        mugRb.linearVelocity = new Vector2(
            Mathf.Clamp(
                mugRb.linearVelocity.x,
                -maxSlideSpeed,
                maxSlideSpeed
            ),
            mugRb.linearVelocity.y
        );
    }

    void Update()
    {
        // Level 2 only: track how long mug has been on this slippery platform
        if (!level2ModeActive || !mugOnPlatform) return;

        slipperyTimer += Time.deltaTime;

        if (slipperyTimer >= levelManager2.slipperyTimeThreshold)
        {
            levelManager2.OnSlipperyPenaltyTriggered();
            slipperyTimer = 0f; // reset so it can trigger again if they stay
        }
    }

    bool IsSlipperyShelf() => CompareTag("Slippery Shelf");

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsSlipperyShelf())
            return;

        if (col.gameObject.CompareTag("Mug"))
        {
            mugOnPlatform = true;
            mugRb = col.gameObject.GetComponent<Rigidbody2D>();
            mugController = col.gameObject.GetComponent<MugController>();

            if (mugController != null)
                mugController.SetSlippery(true);

            // Level 2: notify manager and reset timer
            if (level2ModeActive)
            {
                slipperyTimer = 0f;
                levelManager2.OnEnterSlipperyPlatform();
            }
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (!IsSlipperyShelf())
            return;

        if (col.gameObject.CompareTag("Mug"))
        {
            mugOnPlatform = false;
            mugRb = null;

            if (mugController != null)
                mugController.SetSlippery(false);
            mugController = null;

            // Level 2: notify manager and reset timer
            if (level2ModeActive)
            {
                slipperyTimer = 0f;
                levelManager2.OnExitSlipperyPlatform();
            }
        }
    }
}