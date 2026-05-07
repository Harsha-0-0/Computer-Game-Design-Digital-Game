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

    private bool isUnlocked = false;
    private bool mugEntered = false;
    private SpriteRenderer[] doorRenderers;

    // ── Level 2 only ──────────────────────────────────────────────────────────
    private LevelManager_2 levelManager2;
    private bool level2ModeActive = false;
    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        doorRenderers = GetComponentsInChildren<SpriteRenderer>();

        levelManager2     = FindObjectOfType<LevelManager_2>();
        level2ModeActive  = levelManager2 != null;

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Level1" && !requireAllBeans)
            isUnlocked = true;
    }

    void Update()
    {
        if (isUnlocked) return;

        // ── Level 2: unlock ONLY at exactly 40 drops — over or under = locked ──
        if (level2ModeActive)
        {
            if (levelManager2.IsExactTarget())
            {
                isUnlocked = true;
                Debug.Log("[DoorToNextLevel] Unlocked — exactly 40 milk drops!");
                StartCoroutine(GlowDoor());
            }
            // If player had exactly 40 but then collects more, re-lock the door
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
                Debug.Log("Tutorial door unlocked with beans!");
                StartCoroutine(GlowDoor());
            }
            return;
        }

        // ── Normal levels ─────────────────────────────────────────────────────
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Level1")
        {
            if (requireAllBeans && LevelManager.Instance != null)
            {
                if (LevelManager.Instance.GetBeans() >= beansRequired)
                {
                    isUnlocked = true;
                    Debug.Log("Door unlocked with beans!");
                    StartCoroutine(GlowDoor());
                }
            }
        }
        else if (currentScene == "level 4")
        {
            if (LevelManager.Instance != null)
            {
                if (LevelManager.Instance.GetChocolate() >= LevelManager.Instance.GetRequiredChocolate())
                {
                    isUnlocked = true;
                    Debug.Log("Door unlocked with chocolate!");
                    StartCoroutine(GlowDoor());
                }
            }
        }
        else
        {
            if (LevelManager.Instance != null)
            {
                if (LevelManager.Instance.GetFoam() >= foamRequired)
                {
                    isUnlocked = true;
                    Debug.Log("Door unlocked with foam!");
                    StartCoroutine(GlowDoor());
                }
            }
        }
    }

    IEnumerator GlowDoor()
    {
        while (isUnlocked && !mugEntered)
        {
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

        if (isUnlocked)
        {
            mugEntered = true;
            StartCoroutine(DoorSequence(other.gameObject));
        }
        else
        {
            StartCoroutine(ShowLockedMessage());
        }
    }

    // ── Re-lock if player somehow gains drops after reaching exactly 40 ───────
    // (e.g. walks into a spawned drop while standing near the door)
    void OnTriggerStay2D(Collider2D other)
    {
        if (!level2ModeActive || mugEntered) return;

        // If door was unlocked but player is now over/under target, re-lock it
        if (isUnlocked && !levelManager2.IsExactTarget())
        {
            isUnlocked = false;
            StopAllCoroutines(); // stop glow
            foreach (var sr in doorRenderers)
                if (sr != null) sr.color = Color.white;
            Debug.Log("[DoorToNextLevel] Re-locked — milk drop count changed.");
        }
    }

    IEnumerator DoorSequence(GameObject mug)
    {
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

        SpriteRenderer[] mugRenderers = mug.GetComponentsInChildren<SpriteRenderer>();
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

        // ── Level 2: show exact count feedback ────────────────────────────
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
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == "level 4")
                text.text = "Collect " + LevelManager.Instance.GetRequiredChocolate() + " chocolate particles first!";
            else if (currentScene == "Level1")
                text.text = "Collect all beans first!";
            else
                text.text = "Collect all foam first!";
        }

        yield return new WaitForSeconds(2f);
        Destroy(popup);
    }
}