using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class DoorToNextLevel : MonoBehaviour
{
    [Header("Door Settings")]
    public string nextSceneName = "Level2";
    public bool requireAllBeans = true;
    public int beansRequired = 20;
    public int foamRequired = 20;

    [Header("Blink Settings")]
    public float blinkSpeed = 0.1f;
    public int blinkCount = 6;

    [Header("Level Complete UI")]
    public GameObject levelCompletePanel;

    [Header("Audio")]
    public AudioClip doorOpenSound;
    public AudioClip doorLockedSound;

    private bool isUnlocked = false;
    private bool mugEntered = false;
    private SpriteRenderer[] doorRenderers;
    private AudioSource audioSource;

    // ── Level 2 only ──────────────────────────────────────────────────────────
    private LevelManager_2 levelManager2;
    private bool level2ModeActive = false;
    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        doorRenderers = GetComponentsInChildren<SpriteRenderer>();
        audioSource = gameObject.AddComponent<AudioSource>();

        levelManager2 = FindObjectOfType<LevelManager_2>();
        level2ModeActive = levelManager2 != null;

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Level1" && !requireAllBeans)
            isUnlocked = true;
    }

    void Update()
{
    if (mugEntered) return;

    // ── Level 2 ───────────────────────────────────────────────────────────
    if (level2ModeActive)
    {
        bool exactTarget = levelManager2.IsExactTarget();

        if (exactTarget && !isUnlocked)
        {
            isUnlocked = true;
            Debug.Log("[DoorToNextLevel] Unlocked — exactly 40 milk drops!");
            StartCoroutine(GlowDoor());
        }
        else if (!exactTarget && isUnlocked)
        {
            isUnlocked = false;
            StopCoroutine("GlowDoor");
            foreach (var sr in doorRenderers)
                if (sr != null) sr.color = Color.white;
            Debug.Log("[DoorToNextLevel] Re-locked — milk drop count changed.");
        }
        return;
    }

    // ── Tutorial ──────────────────────────────────────────────────────────
    bool isTutorial = LevelManager.Instance != null &&
                      LevelManager.Instance.isTutorial;

    if (isTutorial)
    {
        if (LevelManager.Instance.GetBeans() >= beansRequired)
        {
            isUnlocked = true;
            Debug.Log("Tutorial door unlocked!");
            StartCoroutine(GlowDoor());
        }
        return;
    }

    // ── Normal levels — NO early return so re-lock always runs ────────────
    string scene = SceneManager.GetActiveScene().name;

    if (scene == "Level1")
    {
        if (requireAllBeans && LevelManager.Instance != null)
        {
            if (LevelManager.Instance.GetBeans() >= beansRequired && !isUnlocked)
            {
                isUnlocked = true;
                Debug.Log("Door unlocked with beans!");
                StartCoroutine(GlowDoor());
            }
        }
    }
    else if (scene == "level 4")
    {
        if (LevelManager.Instance != null)
        {
            bool exact = LevelManager.Instance.GetChocolate() ==
                         LevelManager.Instance.GetRequiredChocolate();
            if (exact && !isUnlocked)
            {
                isUnlocked = true;
                Debug.Log("Door unlocked with exact chocolate!");
                StartCoroutine(GlowDoor());
            }
            else if (!exact && isUnlocked)
            {
                isUnlocked = false;
                StopCoroutine("GlowDoor");
                foreach (var sr in doorRenderers)
                    if (sr != null) sr.color = Color.white;
            }
        }
    }
    else
    {
        if (LevelManager.Instance != null)
        {
            bool exact = LevelManager.Instance.GetFoam() == foamRequired;
            if (exact && !isUnlocked)
            {
                isUnlocked = true;
                Debug.Log("Door unlocked with exact foam!");
                StartCoroutine(GlowDoor());
            }
            else if (!exact && isUnlocked)
            {
                isUnlocked = false;
                StopCoroutine("GlowDoor");
                foreach (var sr in doorRenderers)
                    if (sr != null) sr.color = Color.white;
            }
        }
    }
}

    IEnumerator GlowDoor()
    {
        while (!mugEntered)
        {
            if (!isUnlocked)
            {
                foreach (var sr in doorRenderers)
                    if (sr != null) sr.color = Color.white;
                yield break;
            }

            foreach (var sr in doorRenderers)
                if (sr != null) sr.color = new Color(1f, 0.9f, 0.3f);
            yield return new WaitForSeconds(0.5f);

            foreach (var sr in doorRenderers)
                if (sr != null) sr.color = Color.white;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Mug") || mugEntered) return;

        // ── Level 2: re-check exact target at moment of entry ─────────────
        if (level2ModeActive)
        {
            if (levelManager2.IsExactTarget())
            {
                mugEntered = true;
                StartCoroutine(DoorSequence(other.gameObject));
            }
            else
            {
                if (doorLockedSound != null)
                    audioSource.PlayOneShot(doorLockedSound);
                StartCoroutine(ShowLockedMessage());
            }
            return;
        }

        // ── All other levels ──────────────────────────────────────────────
        if (isUnlocked)
        {
            mugEntered = true;
            StartCoroutine(DoorSequence(other.gameObject));
        }
        else
        {
            if (doorLockedSound != null)
                audioSource.PlayOneShot(doorLockedSound);
            StartCoroutine(ShowLockedMessage());
        }
    }

    IEnumerator DoorSequence(GameObject mug)
    {
        // Play door open sound at the start of the sequence
        if (doorOpenSound != null)
            audioSource.PlayOneShot(doorOpenSound);

        MugController mc = mug.GetComponent<MugController>();
        if (mc != null) mc.enabled = false;

        Rigidbody2D rb = mug.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        for (int i = 0; i < blinkCount; i++)
        {
            foreach (var sr in doorRenderers)
                if (sr != null) sr.enabled = false;
            yield return new WaitForSeconds(blinkSpeed);

            foreach (var sr in doorRenderers)
                if (sr != null) sr.enabled = true;
            yield return new WaitForSeconds(blinkSpeed);
        }

        SpriteRenderer[] mugRenderers =
            mug.GetComponentsInChildren<SpriteRenderer>();

        for (int i = 0; i < blinkCount; i++)
        {
            foreach (var sr in mugRenderers)
                if (sr != null) sr.enabled = false;
            yield return new WaitForSeconds(blinkSpeed);

            foreach (var sr in mugRenderers)
                if (sr != null) sr.enabled = true;
            yield return new WaitForSeconds(blinkSpeed);
        }

        mug.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            yield return new WaitForSeconds(2f);
        }

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator ShowLockedMessage()
    {
        GameObject popup = new GameObject("LockedPopup");
        popup.transform.position = transform.position + new Vector3(0, 2f, 0);

        TextMesh text = popup.AddComponent<TextMesh>();
        text.fontSize = 14;
        text.color = Color.red;
        text.alignment = TextAlignment.Center;
        text.anchor = TextAnchor.MiddleCenter;

        if (level2ModeActive)
        {
            text.text = levelManager2.GetDoorLockedReason();
        }
        else if (LevelManager.Instance != null && LevelManager.Instance.isTutorial)
        {
            text.text = "Collect all beans first!";
        }
        else
{
    string scene = SceneManager.GetActiveScene().name;
    if (scene == "level 4")
    {
        int current = LevelManager.Instance.GetChocolate();
        int required = LevelManager.Instance.GetRequiredChocolate();
        text.text = current < required
            ? $"Need exactly {required} chocolate! ({current}/{required})"
            : $"Too much chocolate! Use slippery platform. ({current}/{required})";
    }
    else if (scene == "Level1")
    {
        text.text = "Collect all beans first!";
    }
    else
    {
        int current = LevelManager.Instance.GetFoam();
        text.text = current < foamRequired
            ? $"Need exactly {foamRequired} foam! ({current}/{foamRequired})"
            : $"Too much foam! Use slippery platform. ({current}/{foamRequired})";
    }
}

        yield return new WaitForSeconds(2f);
        Destroy(popup);
    }
}