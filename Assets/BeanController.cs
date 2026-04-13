using UnityEngine;
using System.Collections;

public class BeanController : MonoBehaviour
{
    public float rollSpeed = 8f;
    public float jumpForce = 18f;
    public float maxSpeed = 8f;
    public float growAmount = 0.1f;      // How much bean grows per collection
    public float growDuration = 0.5f;    // How long the grow animation takes
    public AudioClip jumpSound;
    public AudioClip collectSound;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool inGrinder = false;
    private AudioSource audioSource;
    private Vector3 targetScale;
    private bool isGrowing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = gameObject.AddComponent<AudioSource>();
        targetScale = transform.localScale;
    }

    void Update()
    {
        if (inGrinder) return;

        float move = Input.GetAxisRaw("Horizontal");

        if (isGrounded)
        {
            rb.AddForce(new Vector2(move * rollSpeed, 0));
        }
        else
        {
            float airControlSpeed = 0.08f;
            float targetX = rb.linearVelocity.x + move * airControlSpeed;
            float clampedX = Mathf.Clamp(targetX, -maxSpeed, maxSpeed);
            rb.linearVelocity = new Vector2(clampedX, rb.linearVelocity.y);
        }

        float finalClampedX = Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed);
        rb.linearVelocity = new Vector2(finalClampedX, rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            if (jumpSound != null)
                audioSource.PlayOneShot(jumpSound);
        }

        // Smoothly grow towards target scale
        if (isGrowing)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime / growDuration
            );

            if (Vector3.Distance(transform.localScale, targetScale) < 0.01f)
            {
                transform.localScale = targetScale;
                isGrowing = false;
            }
        }
    }

    public void GrowBean()
    {
        targetScale += new Vector3(growAmount, growAmount, 0);
        isGrowing = true;

        if (collectSound != null)
            audioSource.PlayOneShot(collectSound);
    }

    public void EnterGrinder()
    {
        inGrinder = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
    }

    public bool IsInGrinder() { return inGrinder; }

    void OnCollisionEnter2D(Collision2D col) { isGrounded = true; }
    void OnCollisionExit2D(Collision2D col) { isGrounded = false; }
}