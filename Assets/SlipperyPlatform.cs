using UnityEngine;

public class SlipperyPlatform : MonoBehaviour
{
    [Header("Slippery Settings")]
    public float slideForce = 3f;
    public float controlForce = 4f;
    public float maxSlideSpeed = 6f;
    public float slideDirection = 1f;

    [Header("Audio")]
    public AudioClip slideSound;
    [Range(0f, 1f)] public float slideVolume = 0.6f;

    private bool mugOnPlatform = false;
    private Rigidbody2D mugRb;
    private MugController mugController;
    private AudioSource audioSource;

    // ── Level 2 milk spill (only active when LevelManager_2 exists) ──────────
    private LevelManager_2 levelManager2;
    private float slipperyTimer = 0f;
    private bool level2ModeActive = false;
    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        levelManager2 = FindObjectOfType<LevelManager_2>();
        level2ModeActive = levelManager2 != null;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = slideSound;
        audioSource.loop = true;
        audioSource.volume = slideVolume;
        audioSource.playOnAwake = false;
    }

    void FixedUpdate()
    {
        if (!mugOnPlatform || mugRb == null) return;

        float input = Input.GetAxis("Horizontal");

        mugRb.AddForce(new Vector2(slideForce * slideDirection, 0f));

        if (Mathf.Abs(input) > 0.1f)
            mugRb.AddForce(new Vector2(input * controlForce, 0f));

        mugRb.linearVelocity = new Vector2(
            Mathf.Clamp(mugRb.linearVelocity.x, -maxSlideSpeed, maxSlideSpeed),
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
            slipperyTimer = 0f;
        }
    }

    bool IsSlipperyShelf() => CompareTag("Slippery Shelf");

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsSlipperyShelf()) return;

        if (col.gameObject.CompareTag("Mug"))
        {
            mugOnPlatform = true;
            mugRb = col.gameObject.GetComponent<Rigidbody2D>();
            mugController = col.gameObject.GetComponent<MugController>();

            if (mugController != null)
                mugController.SetSlippery(true);

            if (level2ModeActive)
            {
                slipperyTimer = 0f;
                levelManager2.OnEnterSlipperyPlatform();
            }

            // Start looping slide sound
            if (slideSound != null)
                audioSource.Play();
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (!IsSlipperyShelf()) return;

        if (col.gameObject.CompareTag("Mug"))
        {
            mugOnPlatform = false;
            mugRb = null;

            if (mugController != null)
                mugController.SetSlippery(false);
            mugController = null;

            if (level2ModeActive)
            {
                slipperyTimer = 0f;
                levelManager2.OnExitSlipperyPlatform();
            }

            // Stop slide sound
            if (slideSound != null)
                audioSource.Stop();
        }
    }
}