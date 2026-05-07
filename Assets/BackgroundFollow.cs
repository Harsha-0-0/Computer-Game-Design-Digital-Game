using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxSpeed = 0.1f; // 0 = locked to camera, 1 = moves with world

    private Vector3 lastCameraPos;
    private float spriteWidth;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPos = cameraTransform.position;

        FitToCamera();

        // Store the width after scaling so we know when to reposition
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            spriteWidth = sr.bounds.size.x;
    }

    void FitToCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth  = camHeight * cam.aspect;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        float spriteNativeWidth  = sr.sprite.bounds.size.x;
        float spriteNativeHeight = sr.sprite.bounds.size.y;

        // Scale to fill height, and 3x camera width so there's always
        // coverage on both sides as the camera scrolls right
        transform.localScale = new Vector3(
            (camWidth * 3f) / spriteNativeWidth,
            camHeight / spriteNativeHeight,
            1f
        );

        // Start centred on camera
        transform.position = new Vector3(
            cameraTransform.position.x,
            cameraTransform.position.y,
            transform.position.z
        );
    }

    void LateUpdate()
    {
        Vector3 delta = cameraTransform.position - lastCameraPos;
        transform.position += new Vector3(delta.x * (1 - parallaxSpeed), 0, 0);
        lastCameraPos = cameraTransform.position;

        // If background has drifted too far from camera, recentre it
        // This prevents the void appearing on very long levels
        float distanceFromCamera = transform.position.x - cameraTransform.position.x;
        if (Mathf.Abs(distanceFromCamera) > spriteWidth * 0.25f)
        {
            transform.position = new Vector3(
                cameraTransform.position.x,
                transform.position.y,
                transform.position.z
            );
        }
    }
}