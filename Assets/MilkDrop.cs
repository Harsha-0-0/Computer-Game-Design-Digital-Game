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
        if (collision.gameObject.CompareTag("Mug"))
        {
            LevelManager.Instance.MilkCollected(1);
            Destroy(gameObject);
        }
        Destroy(gameObject);
    }
}
