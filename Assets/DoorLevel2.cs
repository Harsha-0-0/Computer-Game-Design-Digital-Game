using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorLevel2 : MonoBehaviour
{
    [Header("Settings")]
    public int requiredMilk = 40;
    public string nextSceneName = "TitleScene";

    [Header("UI")]
    public GameObject hintUI;
    public float hintDuration = 1.5f;

    [Header("Animation")]
    public float blinkSpeed = 0.1f;
    public int blinkCount = 6;

    private bool entered = false;
    private Coroutine hintCoroutine;
    private SpriteRenderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();

        if (hintUI != null)
            hintUI.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Mug")) return;
        if (entered) return;

        if (LevelManager.Instance == null) return;

        int milk = LevelManager.Instance.GetMilk();

        if (milk == requiredMilk)
        {
            entered = true;
            StartCoroutine(DoorSequence(other.gameObject));
        }
        else if (milk < requiredMilk)
        {
            ShowHint("Need exactly 40 Milk!");
        }
        else
        {
            ShowHint("Too much Milk! Lose some first!");
        }
    }

    void ShowHint(string msg)
    {
        Debug.Log(msg);

        if (hintUI == null) return;

        TMPro.TextMeshProUGUI text =
            hintUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();

        if (text != null)
            text.text = msg;

        if (hintCoroutine != null)
            StopCoroutine(hintCoroutine);

        hintCoroutine = StartCoroutine(HintRoutine());
    }

    IEnumerator HintRoutine()
    {
        hintUI.SetActive(true);
        yield return new WaitForSeconds(hintDuration);
        hintUI.SetActive(false);
        hintCoroutine = null;
    }

    IEnumerator DoorSequence(GameObject mug)
    {
        MugController mc = mug.GetComponent<MugController>();
        if (mc != null)
            mc.enabled = false;

        for (int i = 0; i < blinkCount; i++)
        {
            foreach (var sr in renderers)
                if (sr != null)
                    sr.enabled = false;

            yield return new WaitForSeconds(blinkSpeed);

            foreach (var sr in renderers)
                if (sr != null)
                    sr.enabled = true;

            yield return new WaitForSeconds(blinkSpeed);
        }

        mug.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(nextSceneName);
    }
}