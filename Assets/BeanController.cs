using UnityEngine;

public class BeanController : MonoBehaviour
{
    public float rollSpeed = 5f;
    public float jumpForce = 8f;
    public float maxSpeed = 4f;
    public AudioClip jumpSound;        // drag your jump sound here
    public AudioClip grinderSound;     // drag your grinder sound here

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool inGrinder = false;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (inGrinder) return;

        float move = Input.GetAxis("Horizontal");
        rb.AddForce(new Vector2(move * rollSpeed, 0));

        float clampedX = Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed);
        rb.linearVelocity = new Vector2(clampedX, rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            if (jumpSound != null)
                audioSource.PlayOneShot(jumpSound);
        }
    }

    public void EnterGrinder()
{
    inGrinder = true;
    rb.linearVelocity = Vector2.zero;
    rb.gravityScale = 0f;
}
public bool IsInGrinder()
{
    return inGrinder;
}

    void OnCollisionEnter2D(Collision2D col) { isGrounded = true; }
    void OnCollisionExit2D(Collision2D col) { isGrounded = false; }
}