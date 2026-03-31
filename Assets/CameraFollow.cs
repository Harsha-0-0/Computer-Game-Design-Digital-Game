using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.1f;
    public float verticalOffset = 1.5f; // Camera looks slightly above the bean

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(
            target.position.x,
            target.position.y + verticalOffset,
            -10
        );
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed);
    }
}