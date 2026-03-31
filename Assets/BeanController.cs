using UnityEngine;

public class BeanController : MonoBehaviour
{
    public float rollSpeed = 5f;
    public float jumpForce = 8f;
    public float maxSpeed = 4f;
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool inGrinder = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (inGrinder) return; // Disable player control once inside grinder

        float move = Input.GetAxis("Horizontal");
        rb.AddForce(new Vector2(move * rollSpeed, 0));

        float clampedX = Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed);
        rb.linearVelocity = new Vector2(clampedX, rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        isGrounded = false;
    }

    // Called by GrinderGoal when bean enters grinder trigger
    public void EnterGrinder()
    {
        inGrinder = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
    }
}