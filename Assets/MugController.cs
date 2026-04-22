using UnityEngine;

public class MugController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isSlippery = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Skip horizontal movement if slippery
        // SlipperyPlatform handles it
        if (!isSlippery)
        {
            float move = 0f;

            if (Input.GetKey(KeyCode.RightArrow) ||
                Input.GetKey(KeyCode.D))
                move = 1f;

            if (Input.GetKey(KeyCode.LeftArrow) ||
                Input.GetKey(KeyCode.A))
                move = -1f;

            rb.linearVelocity = new Vector2(
                move * moveSpeed,
                rb.linearVelocity.y
            );
        }

        // Jump always works
        if ((Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
            && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }
    }

    public void SetSlippery(bool slippery)
    {
        isSlippery = slippery;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}