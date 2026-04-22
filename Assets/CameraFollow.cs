using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.05f;
    public float horizontalOffset = 3f;
    public float verticalOffset = 2f;
    public float cameraSize = 12f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
            cam.orthographicSize = cameraSize;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(
            target.position.x + horizontalOffset,
            target.position.y + verticalOffset,
            -10
        );
        transform.position = Vector3.Lerp(
            transform.position,
            desired,
            smoothSpeed
        );
    }
}