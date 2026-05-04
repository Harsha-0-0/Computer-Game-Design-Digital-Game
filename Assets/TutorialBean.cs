using UnityEngine;

public class TutorialBean : MonoBehaviour
{
    [Header("Circle Movement")]
    public float circleRadius = 0.2f;
    public float circleSpeed = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Move in small circle
        float x = startPos.x +
            Mathf.Cos(Time.time * circleSpeed)
            * circleRadius;
        float y = startPos.y +
            Mathf.Sin(Time.time * circleSpeed)
            * circleRadius;

        transform.position = new Vector3(
            x, y, startPos.z
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mug"))
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.BeanCollected();
            Destroy(gameObject);
        }
    }
}