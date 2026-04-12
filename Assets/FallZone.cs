using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FallZone : MonoBehaviour
{
    public AudioClip fallSound;
    private bool isRestarting = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Bean") && !isRestarting)
        {
            isRestarting = true;
            StartCoroutine(FallAndRestart(col.gameObject));
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Bean") && !isRestarting)
        {
            isRestarting = true;
            StartCoroutine(FallAndRestart(col.gameObject));
        }
    }

    IEnumerator FallAndRestart(GameObject bean)
    {
        // Play fall sound
        if (fallSound != null)
        {
            AudioSource.PlayClipAtPoint(
                fallSound,
                bean.transform.position
            );
            yield return new WaitForSeconds(fallSound.length);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}