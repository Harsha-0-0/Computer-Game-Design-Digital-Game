using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelDoor : MonoBehaviour
{
    public float openAngle = -90f;
    public float openSpeed = 2f;
    public GameObject level2Screen;
    public AudioClip doorOpenSound;

    // New: hint UI
    public GameObject lockedHintUI;
    public float hintDuration = 1.5f;

    private bool isUnlocked = false;
    private bool isOpening = false;
    private bool beanEnteredDoor = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;
    private Coroutine hintCoroutine;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(0, 0, openAngle);
        audioSource = gameObject.AddComponent<AudioSource>();

        if (lockedHintUI != null)
            lockedHintUI.SetActive(false);
    }

    void Update()
    {
        if (isOpening)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                openRotation,
                Time.deltaTime * openSpeed
            );
        }
    }

    public void Unlock()
    {
        isUnlocked = true;
        Debug.Log("Door unlocked!");

        if (lockedHintUI != null)
            lockedHintUI.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something entered door: " + other.gameObject.name);

        if (other.CompareTag("Bean") && isUnlocked && !beanEnteredDoor)
        {
            Debug.Log("Bean entered door!");
            beanEnteredDoor = true;
            StartCoroutine(DoorSequence(other.gameObject));
        }
        else if (other.CompareTag("Bean") && !isUnlocked)
        {
            Debug.Log("Bean reached door but not unlocked yet!");
            ShowLockedHint();
        }
    }

    void ShowLockedHint()
    {
        if (lockedHintUI == null) return;

        if (hintCoroutine != null)
            StopCoroutine(hintCoroutine);

        hintCoroutine = StartCoroutine(ShowHintTemporarily());
    }

    IEnumerator ShowHintTemporarily()
    {
        lockedHintUI.SetActive(true);
        yield return new WaitForSeconds(hintDuration);
        lockedHintUI.SetActive(false);
        hintCoroutine = null;
    }

    IEnumerator DoorSequence(GameObject bean)
    {
        if (doorOpenSound != null)
            audioSource.PlayOneShot(doorOpenSound);

        isOpening = true;

        yield return new WaitForSeconds(0.8f);

        SpriteRenderer sr = bean.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float elapsed = 0f;
            float fadeDuration = 0.5f;
            Color c = sr.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                sr.color = c;
                yield return null;
            }
        }

        bean.SetActive(false);

        yield return new WaitForSeconds(0.3f);

        if (level2Screen != null)
        {
            Debug.Log("Activating Level2Canvas!");
            level2Screen.SetActive(true);
            WinSequence ws = level2Screen.GetComponent<WinSequence>();
            if (ws != null)
            {
                Debug.Log("WinSequence found! Playing...");
                ws.Play();
            }
            else
            {
                Debug.Log("WinSequence NOT found on Level2Canvas!");
            }
        }
        else
        {
            Debug.Log("level2Screen is NULL - not assigned!");
        }
    }
}