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

    void Start()
    {
        doorRenderers = GetComponentsInChildren
            <SpriteRenderer>();

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Level1" && !requireAllBeans)
            isUnlocked = true;
    }

    void Update()
    {
        if (isUnlocked) return;

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Level1")
        {
            // Require beans for Level1
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
            // Require required chocolate for Level 4
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
            // Require foam for other levels
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
        // Pulse the door color to show
        // it's now unlocked
        while (isUnlocked && !mugEntered)
        {
            // Glow bright
            foreach (var sr in doorRenderers)
            {
                if (sr != null)
                    sr.color = new Color(
                        1f, 0.9f, 0.3f
                    );
            }
            yield return new WaitForSeconds(0.5f);

            // Normal color
            foreach (var sr in doorRenderers)
            {
                if (sr != null)
                    sr.color = Color.white;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mug") &&
            !mugEntered)
        {
            if (isUnlocked)
            {
                mugEntered = true;
                StartCoroutine(
                    DoorSequence(other.gameObject)
                );
            }
            else
            {
                StartCoroutine(ShowLockedMessage());
            }
        }
    }

    IEnumerator DoorSequence(GameObject mug)
    {
        // Disable mug movement
        MugController mc = mug
            .GetComponent<MugController>();
        if (mc != null)
            mc.enabled = false;

        // Stop any residual movement
        Rigidbody2D rb = mug.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Blink the door rapidly
        for (int i = 0; i < blinkCount; i++)
        {
            // Hide door
            foreach (var sr in doorRenderers)
                if (sr != null)
                    sr.enabled = false;
            yield return new WaitForSeconds(
                blinkSpeed
            );

            // Show door
            foreach (var sr in doorRenderers)
                if (sr != null)
                    sr.enabled = true;
            yield return new WaitForSeconds(
                blinkSpeed
            );
        }

        // Blink the mug
        SpriteRenderer[] mugRenderers =
            mug.GetComponentsInChildren
                <SpriteRenderer>();

        for (int i = 0; i < blinkCount; i++)
        {
            foreach (var sr in mugRenderers)
                if (sr != null)
                    sr.enabled = false;
            yield return new WaitForSeconds(
                blinkSpeed
            );

            foreach (var sr in mugRenderers)
                if (sr != null)
                    sr.enabled = true;
            yield return new WaitForSeconds(
                blinkSpeed
            );
        }

        // Hide mug completely
        mug.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // Show level complete panel if exists
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            yield return new WaitForSeconds(2f);
        }

        // Load next scene
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator ShowLockedMessage()
    {
        GameObject popup =
            new GameObject("LockedPopup");
        popup.transform.position =
            transform.position +
            new Vector3(0, 2f, 0);

        TextMesh text =
            popup.AddComponent<TextMesh>();
        text.fontSize = 14;
        text.color = Color.red;
        text.alignment = TextAlignment.Center;
        text.anchor = TextAnchor.MiddleCenter;

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Level1")
        {
            text.text = "Collect all beans first!";
        }
        else if (currentScene == "level 4")
        {
            text.text = "Collect " + LevelManager.Instance.GetRequiredChocolate() + " chocolate particles first!";
        }
        else
        {
            text.text = "Collect all foam first!";
        }

        yield return new WaitForSeconds(2f);
        Destroy(popup);
    }
}