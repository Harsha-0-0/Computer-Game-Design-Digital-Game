using UnityEngine;

/// <summary>
/// Represents a single milk drop collectible in Level 2.
/// Handles both platform-placed drops and spawner-dropped drops.
/// Attach to each milk drop prefab.
/// </summary>
public class MilkDrop_2 : MonoBehaviour
{
    [Header("Drop Settings")]
    [Tooltip("How fast the drop falls when spawned from above")]
    public float fallSpeed = 3f;

    [Tooltip("Is this drop spawned from the sky (true) or placed on a platform (false)?")]
    public bool isSpawned = false;

    [Tooltip("Auto-destroy after this many seconds if never collected (0 = never)")]
    public float autoDestroyTime = 10f;

    [Header("Visual Feedback")]
    [Tooltip("Optional particle effect played when collected")]
    public GameObject collectParticlePrefab;

    [Tooltip("Optional sound played when collected")]
    public AudioClip collectSound;

    private Rigidbody2D rb;
    private bool collected = false;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (isSpawned && rb != null)
        {
            // Spawned drops fall via gravity — make sure Rigidbody2D gravity scale is set
            rb.gravityScale = 1f;
        }

        if (autoDestroyTime > 0f && isSpawned)
        {
            Destroy(gameObject, autoDestroyTime);
        }
    }

    /// <summary>
    /// Called when the mug touches this milk drop.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Mug"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        collected = true;

        // Notify the Level 2 game manager
        LevelManager_2 levelManager = FindObjectOfType<LevelManager_2>();
        if (levelManager != null)
        {
            levelManager.AddMilkDrop();
        }

        // Visual feedback
        if (collectParticlePrefab != null)
        {
            Instantiate(collectParticlePrefab, transform.position, Quaternion.identity);
        }

        // Sound feedback
        if (collectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        // Hide sprite immediately, destroy after sound finishes
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, collectSound != null ? collectSound.length : 0f);
    }

    /// <summary>
    /// Called externally by SlipperyPlatform logic to remove a drop from the player's count.
    /// This destroys a platform-placed drop that was "spilled" back into the scene (optional visual).
    /// If you want a spill animation, instantiate a prefab here before destroying.
    /// </summary>
    public static void SpillDrops(int amount, LevelManager_2 levelManager)
    {
        if (levelManager != null)
        {
            levelManager.RemoveMilkDrops(amount);
        }
    }
}