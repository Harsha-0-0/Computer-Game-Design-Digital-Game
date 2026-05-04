using UnityEngine;

public class SlipperyPlatform : MonoBehaviour
{
    [Header("Slippery Settings")]
    public float slideForce = 3f;
    public float controlForce = 4f;
    public float maxSlideSpeed = 6f;
    public float slideDirection = 1f;
    // 1 = slides right, -1 = slides left

    private bool mugOnPlatform = false;
    private Rigidbody2D mugRb;
    private MugController mugController;

    void FixedUpdate()
    {
        if (!mugOnPlatform || mugRb == null) return;

        float input = Input.GetAxis("Horizontal");
        float currentX = mugRb.linearVelocity.x;

        // Always push mug in slide direction
        // even when standing still
        mugRb.AddForce(
            new Vector2(slideForce * slideDirection, 0f)
        );

        // Player can push back but with less force
        if (Mathf.Abs(input) > 0.1f)
        {
            mugRb.AddForce(
                new Vector2(input * controlForce, 0f)
            );
        }

        // Clamp max speed
        mugRb.linearVelocity = new Vector2(
            Mathf.Clamp(
                mugRb.linearVelocity.x,
                -maxSlideSpeed,
                maxSlideSpeed
            ),
            mugRb.linearVelocity.y
        );
    }

    bool IsSlipperyShelf() => CompareTag("Slippery Shelf");

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsSlipperyShelf())
            return;

        if (col.gameObject.CompareTag("Mug"))
        {
            mugOnPlatform = true;
            mugRb = col.gameObject
                .GetComponent<Rigidbody2D>();
            mugController = col.gameObject
                .GetComponent<MugController>();
            if (mugController != null)
                mugController.SetSlippery(true);
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (!IsSlipperyShelf())
            return;

        if (col.gameObject.CompareTag("Mug"))
        {
            mugOnPlatform = false;
            mugRb = null;
            if (mugController != null)
                mugController.SetSlippery(false);
            mugController = null;
        }
    }
}