using UnityEngine;

public class MilkDrop : MonoBehaviour
{
    public int milkValue = 1;
    public float autoDestroyTime = 5f;
    private bool collected = false;

    void Start()
    {
        Destroy(gameObject, autoDestroyTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mug") && !collected)
        {
            collected = true;
            if (LevelManager.Instance != null)
                LevelManager.Instance.MilkCollected(milkValue);
            Destroy(gameObject);
        }
        // Removed the stray Destroy here — milk no longer
        // destroys itself when hitting platforms/ground
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mug") && !collected)
        {
            collected = true;
            if (LevelManager.Instance != null)
                LevelManager.Instance.MilkCollected(milkValue);
            Destroy(gameObject);
        }
    }
}