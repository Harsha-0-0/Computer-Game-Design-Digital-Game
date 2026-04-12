using UnityEngine;

public class CollectibleBean : MonoBehaviour
{
    public float bobSpeed = 2f;       // floating bob up and down
    public float bobHeight = 0.2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Bob up and down so it's visible and attractive
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bean"))
        {
            // Tell GameManager a bean was collected
            GameManager.Instance.BeanCollected();

            // Tell the player bean to grow
            BeanController bc = other.GetComponent<BeanController>();
            if (bc != null) bc.GrowBean();

            // Destroy this collectible
            Destroy(gameObject);
        }
    }
}