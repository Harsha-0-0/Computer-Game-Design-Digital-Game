using UnityEngine;

public class MovingMilkSpawner : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDistance = 4f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float xOffset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;

        transform.position = new Vector3(
            startPosition.x + xOffset,
            startPosition.y,
            startPosition.z
        );
    }
}