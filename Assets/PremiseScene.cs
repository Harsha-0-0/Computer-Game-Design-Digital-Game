using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class PremiseScene : MonoBehaviour
{
    [Header("Settings")]
    public string nextSceneName = "MugSelectionScene";
    public float autoAdvanceTime = 5f;  // Auto moves on after this
    public float fadeDuration = 1f;

    [Header("Optional Skip Text")]
    public TextMeshProUGUI skipText;    // Optional — assign if you have one

    private bool isTransitioning = false;

    void Start()
    {
        if (skipText != null)
            skipText.text = "Press any key to continue";

        // Auto advance after set time
        StartCoroutine(AutoAdvance());
    }

    void Update()
    {
        // Any key or mouse click advances immediately
        if (!isTransitioning &&
            (Input.anyKeyDown ||
             Input.GetMouseButtonDown(0)))
        {
            StartCoroutine(Transition());
        }
    }

    IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(autoAdvanceTime);
        if (!isTransitioning)
            StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        isTransitioning = true;
        yield return new WaitForSeconds(fadeDuration);
        SceneManager.LoadScene(nextSceneName);
    }
}