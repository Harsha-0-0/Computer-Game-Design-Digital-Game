using UnityEngine;

public class SlipperyPlatform : MonoBehaviour
{
    [Header("Slippery Settings")]
    public float slideForce = 3f;
    public float controlForce = 4f;
    public float maxSlideSpeed = 6f;
    public float slideDirection = 1f;

    [Header("Slippery Penalty (Level 3 / Level 4)")]
    [Tooltip("Seconds before penalty triggers")]
    public float slipperyTimeThreshold = 1f;
    [Tooltip("Amount of foam/chocolate lost per penalty")]
    public int slipperyPenaltyAmount = 1;
    [Tooltip("Cooldown between penalties")]
    public float slipperyCooldown = 0.3f;

    [Header("Audio")]
    public AudioClip slideSound;
    [Range(0f, 1f)] public float slideVolume = 0.6f;

    private bool mugOnPlatform = false;
    private Rigidbody2D mugRb;
    private MugController mugController;
    private AudioSource audioSource;

    // ── Level 2 ───────────────────────────────────────────────────────────────
    private LevelManager_2 levelManager2;
    private bool level2ModeActive = false;

    // ── Level 3 / Level 4 (LevelManager with foam or chocolate system) ────────
    private LevelManager levelManager;
    private bool penaltyModeActive = false;
    private float slipperyTimer = 0f;
    private bool slipperyCooldownActive = false;

    void Start()
    {
        levelManager2 = FindObjectOfType<LevelManager_2>();
        level2ModeActive = levelManager2 != null;

        if (!level2ModeActive)
        {
            levelManager = LevelManager.Instance;
            // Active if either foam or chocolate system is on
            if (levelManager != null &&
                (levelManager.useFoamSystem || levelManager.useChocolateSystem))
                penaltyModeActive = true;
        }

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
        if (!mugOnPlatform) return;

        // ── Level 2 ───────────────────────────────────────────────────────
        if (level2ModeActive)
        {
            slipperyTimer += Time.deltaTime;
            if (slipperyTimer >= levelManager2.slipperyTimeThreshold)
            {
                levelManager2.OnSlipperyPenaltyTriggered();
                slipperyTimer = 0f;
            }
            return;
        }

        // ── Level 3 (foam) / Level 4 (chocolate) ─────────────────────────
        if (penaltyModeActive)
        {
            slipperyTimer += Time.deltaTime;
            if (slipperyTimer >= slipperyTimeThreshold && !slipperyCooldownActive)
            {
                ApplyPenalty();
                slipperyTimer = 0f;
                StartCoroutine(PenaltyCooldown());
            }
        }
    }

    void ApplyPenalty()
{
    if (levelManager == null) return;

    if (levelManager.useFoamSystem)
    {
        if (levelManager.GetFoam() <= 0) return; // nothing to spill
        levelManager.LoseFoam(slipperyPenaltyAmount);
        Debug.Log($"[SlipperyPlatform] Lost {slipperyPenaltyAmount} foam.");
    }
    else if (levelManager.useChocolateSystem)
    {
        if (levelManager.GetChocolate() <= 0) return; // nothing to spill
        levelManager.LoseChocolate(slipperyPenaltyAmount);
        Debug.Log($"[SlipperyPlatform] Lost {slipperyPenaltyAmount} chocolate.");
    }
}

    System.Collections.IEnumerator PenaltyCooldown()
    {
        slipperyCooldownActive = true;
        yield return new WaitForSeconds(slipperyCooldown);
        slipperyCooldownActive = false;
    }

    bool IsSlipperyShelf() => CompareTag("Slippery Shelf");

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsSlipperyShelf()) return;

        if (col.gameObject.CompareTag("Mug"))
        {
            mugRb = col.gameObject.GetComponent<Rigidbody2D>();
            mugController = col.gameObject.GetComponent<MugController>();

            if (mugController != null)
                mugController.SetSlippery(true);

            if (!mugOnPlatform)
            {
                mugOnPlatform = true;
                slipperyTimer = 0f;

                if (level2ModeActive)
                    levelManager2.OnEnterSlipperyPlatform();

                if (slideSound != null)
                    audioSource.Play();
            }
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (!IsSlipperyShelf()) return;

        if (col.gameObject.CompareTag("Mug"))
        {
            mugOnPlatform = false;
            mugRb = null;
            slipperyTimer = 0f;

            if (mugController != null)
                mugController.SetSlippery(false);
            mugController = null;

            if (level2ModeActive)
                levelManager2.OnExitSlipperyPlatform();

            if (slideSound != null)
                audioSource.Stop();
        }
    }
}