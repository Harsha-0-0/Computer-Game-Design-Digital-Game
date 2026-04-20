using UnityEngine;

public class Level1Bean : MonoBehaviour
{
    public float bobSpeed = 2f;
    public float bobHeight = 0.15f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y +
            Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(
            startPos.x, newY, startPos.z
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mug"))
        {
            LevelManager.Instance.BeanCollected();
            Destroy(gameObject);
        }
    }
}