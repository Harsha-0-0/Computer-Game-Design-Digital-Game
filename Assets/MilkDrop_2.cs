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

    

    private Rigidbody2D rb;
    private bool collected = false;
    [Header("Audio")]
    public AudioClip collectSound;

    

    void Start()
    {
        if (rb != null)
        {
            if (isSpawned)
            {
                // Spawned drops fall via gravity
                rb.gravityScale = 1f;
                rb.bodyType = RigidbodyType2D.Dynamic;
                // Freeze X so drop doesn't slide off platform
                rb.constraints = RigidbodyConstraints2D.FreezeRotation |
                                RigidbodyConstraints2D.FreezePositionX;
            }
            else
            {
                // Fixed platform drops — no gravity, stay in place
                rb.gravityScale = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        if (autoDestroyTime > 0f && isSpawned)
            Destroy(gameObject, autoDestroyTime);
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

        // Check which scene we're in and notify correct manager
        string currentScene = UnityEngine.SceneManagement
            .SceneManager.GetActiveScene().name;

        if (currentScene == "Level_2")
        {
            LevelManager_2 lm2 = FindObjectOfType<LevelManager_2>();
            if (lm2 != null)
            {
                lm2.AddMilkDrop();
                Debug.Log("MilkDrop_2 collected! Notified LevelManager_2.");
            }
            else
                Debug.LogError("MilkDrop_2: LevelManager_2 not found!");
        }
        else
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.MilkCollected(1);
            else
                Debug.LogError("MilkDrop_2: LevelManager.Instance is null!");
        }

        // Visual feedback
        if (collectParticlePrefab != null)
            Instantiate(collectParticlePrefab,
                transform.position, Quaternion.identity);

        // Sound feedback
        if (collectSound != null)
        {
            GameObject soundHost = new GameObject("BeanCollectSound");
            soundHost.transform.position = transform.position;
            AudioSource tempAudio = soundHost.AddComponent<AudioSource>();
            tempAudio.PlayOneShot(collectSound);
            Destroy(soundHost, collectSound.length);
        }


        // Hide sprite immediately
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject,
            collectSound != null ? collectSound.length : 0f);
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