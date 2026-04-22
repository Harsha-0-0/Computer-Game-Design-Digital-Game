using UnityEngine;
using System.Collections;

public class FallZone : MonoBehaviour
{
    private bool isTriggered = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Mug") && !isTriggered)
        {
            isTriggered = true;
            LevelManager.Instance.MugDied();
        }
    }
}