using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxSpeed = 0.1f; // 0 = locked to camera, 1 = moves with world

    private Vector3 lastCameraPos;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPos = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cameraTransform.position - lastCameraPos;
        transform.position += new Vector3(delta.x * (1 - parallaxSpeed), 0, 0);
        lastCameraPos = cameraTransform.position;
    }
}