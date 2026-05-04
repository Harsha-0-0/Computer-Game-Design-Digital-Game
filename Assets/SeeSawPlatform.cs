using UnityEngine;

public class SeesawPlatform : MonoBehaviour
{
    [Header("Seesaw Settings")]
    public float maxTiltAngle = 25f;    // How far it rocks (degrees)
    public float rockSpeed    = 1.5f;   // How fast it rocks back and forth

    [Header("Slide Settings")]
    public float slideForce   = 6f;     // How hard the mug gets pushed downhill
    public float maxSlideSpeed = 5f;    // Cap on sliding speed

    private bool mugOnPlatform = false;
    private Rigidbody2D mugRb;

    void Update()
    {
        // Continuously rock back and forth
        float angle = Mathf.Sin(Time.time * rockSpeed) * maxTiltAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void FixedUpdate()
    {
        if (!mugOnPlatform || mugRb == null) return;

        // Get the current tilt angle in radians
        float tiltAngle = transform.eulerAngles.z;

        // Convert to -180/+180 range
        if (tiltAngle > 180f) tiltAngle -= 360f;

        // Push mug in the direction the platform is tilting
        // Positive angle = tilting left = push mug left
        // Negative angle = tilting right = push mug right
        float slideDirection = -Mathf.Sign(tiltAngle);
        float tiltStrength   = Mathf.Abs(tiltAngle) / maxTiltAngle; // 0 to 1

        mugRb.AddForce(
            new Vector2(slideDirection * slideForce * tiltStrength, 0f),
            ForceMode2D.Force
        );

        // Clamp slide speed so it doesn't go crazy
        mugRb.linearVelocity = new Vector2(
            Mathf.Clamp(mugRb.linearVelocity.x, -maxSlideSpeed, maxSlideSpeed),
            mugRb.linearVelocity.y
        );
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Mug"))
        {
            mugOnPlatform = true;
            mugRb = col.gameObject.GetComponent<Rigidbody2D>();

            // Tell MugController it's on the seesaw
            MugController mc = col.gameObject.GetComponent<MugController>();
            if (mc != null) mc.SetOnSeesaw(true);
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Mug"))
        {
            mugOnPlatform = false;
            mugRb = null;

            // Restore normal control
            MugController mc = col.gameObject.GetComponent<MugController>();
            if (mc != null) mc.SetOnSeesaw(false);
        }
    }
}