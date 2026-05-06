using UnityEngine;
using System.Collections;

public class MatchaPatroller : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float moveSpeed = 2.5f;
    public float edgeBuffer = 0.3f; // how far from edge to turn around

    [Header("Ground Detection")]
    public float groundCheckRadius = 0.2f;
    public Transform groundCheck;
    public string[] groundTags = { "Ground", "Slippery Shelf" };

    [Header("Player Bump")]
    public float bumpForceX = 14f;
    public float bumpForceY = 0f;
    public float bumpCooldown = 0.8f;
    public float bumpDetectRange = 1.2f; // horizontal distance to trigger bump

    [Header("Gap Detection")]
    public float gapProbeDistance = 0.4f;
    public float gapProbeDepth = 1.5f;

    private Rigidbody2D rb;
    private float direction = 1f;
    private bool isGrounded = false;
    private bool isBumping = false;
    private float lastBumpTime = -99f;
    private Transform player;

    private float leftBound;
private float rightBound;
private bool boundsFound = false;

void Start()
{
    rb = GetComponent<Rigidbody2D>();
    rb.gravityScale = 1f;
    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    rb.angularVelocity = 0f;
    rb.sharedMaterial = new PhysicsMaterial2D("MatchaMat")
    {
        bounciness = 0f,
        friction = 0.4f
    };

    if (groundCheck == null)
    {
        GameObject gc = new GameObject("GroundCheck");
        gc.transform.SetParent(transform);
        gc.transform.localPosition = new Vector3(0, -0.65f, 0);
        groundCheck = gc.transform;
    }

    GameObject playerObj = GameObject.FindGameObjectWithTag("Mug");
    if (playerObj != null)
        player = playerObj.transform;

    direction = Random.value > 0.5f ? 1f : -1f;

    // Find the platform Matcha is standing on and record its bounds
    StartCoroutine(FindPlatformBounds());
}

IEnumerator FindPlatformBounds()
{
    // Wait one frame so physics is settled
    yield return new WaitForFixedUpdate();

    Collider2D[] hits = Physics2D.OverlapCircleAll(
        groundCheck.position, groundCheckRadius + 0.3f);

    foreach (Collider2D hit in hits)
    {
        if (hit.gameObject == gameObject) continue;
        foreach (string tag in groundTags)
        {
            if (hit.CompareTag(tag))
            {
                // Use the platform's collider bounds
                Bounds b = hit.bounds;
                leftBound  = b.min.x + edgeBuffer;
                rightBound = b.max.x - edgeBuffer;
                boundsFound = true;
                Debug.Log($"[Matcha] Platform bounds found: {leftBound} to {rightBound}");
                break;
            }
        }
        if (boundsFound) break;
    }

    if (!boundsFound)
        Debug.LogWarning("[Matcha] Could not find platform bounds — gap detection only.");
}

void FixedUpdate()
{
    isGrounded = CheckGrounded();

    if (isBumping) return;

    if (boundsFound)
    {
        // Reverse at platform edges using cached bounds
        if (transform.position.x <= leftBound)
        {
            direction = 1f;
        }
        else if (transform.position.x >= rightBound)
        {
            direction = -1f;
        }
    }
    else
    {
        // Fallback: use gap detection if bounds weren't found
        if (isGrounded && IsGapAhead(direction))
            direction *= -1f;
    }

    rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

    TryBumpPlayer();
}

    void TryBumpPlayer()
    {
        if (player == null) return;
        if (Time.time - lastBumpTime < bumpCooldown) return;

        float distX = Mathf.Abs(player.position.x - transform.position.x);
        float distY = Mathf.Abs(player.position.y - transform.position.y);

        // Only bump if player is horizontally close AND roughly on the same height
        // (same platform = within ~1 unit vertically)
        if (distX < bumpDetectRange && distY < 1.2f)
        {
            lastBumpTime = Time.time;
            StartCoroutine(DoBump(player.gameObject));
        }
    }

    IEnumerator DoBump(GameObject mugObj)
    {
        isBumping = true;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Rigidbody2D playerRb = mugObj.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            float pushDir = Mathf.Sign(mugObj.transform.position.x - transform.position.x);
            playerRb.linearVelocity = Vector2.zero;
            playerRb.AddForce(
                new Vector2(pushDir * bumpForceX, bumpForceY),
                ForceMode2D.Impulse
            );
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.3f);
        isBumping = false;
    }

    bool CheckGrounded()
    {
        if (groundCheck == null) return false;
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            groundCheck.position, groundCheckRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            foreach (string tag in groundTags)
                if (hit.CompareTag(tag)) return true;
        }
        return false;
    }

    bool IsGapAhead(float dirX)
{
    // Start the probe from Matcha's foot level, pushed ahead horizontally
    // Use a longer probe distance so it reaches ahead of the edge
    Vector2 probeOrigin = (Vector2)transform.position
        + new Vector2(dirX * (gapProbeDistance + 0.3f), -0.5f);

    RaycastHit2D[] hits = Physics2D.RaycastAll(
        probeOrigin, Vector2.down, gapProbeDepth + 1.5f);

    Debug.DrawRay(probeOrigin, Vector2.down * (gapProbeDepth + 1.5f),
        Color.red, 0.1f);

    foreach (RaycastHit2D h in hits)
    {
        if (h.collider.gameObject == gameObject) continue;
        foreach (string tag in groundTags)
            if (h.collider.CompareTag(tag)) return false;
    }
    return true;
}

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Vector2 probeOrigin = (Vector2)groundCheck.position
            + new Vector2(direction * gapProbeDistance, 0f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(probeOrigin, probeOrigin + Vector2.down * gapProbeDepth);

        // Show bump detection range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 1.2f);
    }

    void OnCollisionEnter2D(Collision2D col)
{
    // Bump the player mug — don't reverse direction
    if ((col.gameObject.CompareTag("Mug") || col.gameObject.CompareTag("Bean"))
        && Time.time - lastBumpTime > bumpCooldown
        && !isBumping)
    {
        lastBumpTime = Time.time;
        StartCoroutine(DoBump(col.gameObject));
        return; // skip direction reversal entirely
    }

    // Don't reverse during or just after a bump
    if (isBumping) return;
    if (Time.time - lastBumpTime < bumpCooldown) return;

    // Reverse when hitting a non-platform obstacle
    bool isPlatform = false;
    foreach (string tag in groundTags)
    {
        if (col.gameObject.CompareTag(tag))
        {
            isPlatform = true;
            break;
        }
    }

    if (!isPlatform)
        direction *= -1f;
}
}