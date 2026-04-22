using UnityEngine;
using System.Collections;

public class DrainPlatform : MonoBehaviour
{
    public int drainAmount = 1;
    public float drainInterval = 1f;

    private bool mugOnPlatform = false;
    private Coroutine drainCoroutine;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mug"))
        {
            mugOnPlatform = true;

            if (drainCoroutine == null)
            {
                drainCoroutine = StartCoroutine(DrainMilk());
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mug"))
        {
            mugOnPlatform = false;

            if (drainCoroutine != null)
            {
                StopCoroutine(drainCoroutine);
                drainCoroutine = null;
            }
        }
    }

    IEnumerator DrainMilk()
    {
        while (mugOnPlatform)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoseMilk(drainAmount);
            }

            yield return new WaitForSeconds(drainInterval);
        }

        drainCoroutine = null;
    }
}