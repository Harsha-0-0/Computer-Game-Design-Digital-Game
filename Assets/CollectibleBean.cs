using UnityEngine;

public class CollectibleBean : MonoBehaviour
{
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Bob up and down
        float newY = startPos.y +
            Mathf.Sin(Time.time * bobSpeed) 
            * bobHeight;
        transform.position = new Vector3(
            startPos.x, newY, startPos.z
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bean") || 
            other.CompareTag("Mug"))
        {
            // Tell LevelManager bean collected
            if (LevelManager.Instance != null)
                LevelManager.Instance.BeanCollected();
            
            Destroy(gameObject);
        }
    }
}