using UnityEngine;
using System.Collections;

public class MatchaController : MonoBehaviour
{
    [Header("Chase Settings")]
    public Transform player;
    public float moveSpeed  = 3.5f;
    public float startDelay = 3f;

    [Header("Jump Settings")]
    public float jumpForce       = 7f;
    public float jumpThreshold   = 0.8f;
    public float maxJumpDistance = 6f;
    public float jumpCooldown    = 1.2f;

    [Header("Ground Detection")]
    public float     groundCheckRadius = 0.2f;
    public Transform groundCheck;
    public string[]  groundTags = { "Ground", "Platform" };

    [Header("Gap Detection")]
    // How far ahead (in front of matcha's foot) to probe for a gap
    public float gapProbeDistance = 0.5f;
    // How far DOWN the probe ray travels — keep short (just below platform thickness)
    // If no Ground-tagged object is found within this distance, it's a gap
    public float gapProbeDepth = 1.5f;

    [Header("Player Bump")]
    public float bumpForceX   = 14f;
    public float bumpForceY   = 4f;
    public float bumpCooldown = 1.5f;

    // ── Private ───────────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private float startTime;
    private float lastJumpTime = -99f;
    private float lastBumpTime = -99f;
    private bool  isGrounded   = false;
    private bool  isBumping    = false;

    void Start()
    {
        rb        = GetComponent<Rigidbody2D>();
        startTime = Time.time;
        rb.gravityScale  = 1f;
        rb.constraints   = RigidbodyConstraints2D.FreezeRotation;  // Never roll or spin
        rb.angularVelocity = 0f;
        rb.sharedMaterial = new PhysicsMaterial2D("MatchaMat")
        {
            bounciness = 0f,
            friction   = 0.4f
        };

        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -0.55f, 0);
            groundCheck = gc.transform;
        }
    }

    void FixedUpdate()
    {
        isGrounded = CheckGrounded();

        if (isBumping)
        {
            rb.linearVelocity = new Vector2(0f, Mathf.Min(rb.linearVelocity.y, 0f));
            return;
        }

        if (Time.time - startTime < startDelay)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (player == null) return;

        float dirX  = Mathf.Sign(player.position.x - transform.position.x);
        float distX = Mathf.Abs(player.position.x - transform.position.x);
        float distY = player.position.y - transform.position.y;

        // ── Gap detection ─────────────────────────────────────────────────
        bool gapAhead = IsGapAhead(dirX);

        // ── Horizontal movement ───────────────────────────────────────────
        if (!gapAhead || !isGrounded)
        {
            // Safe ground ahead, or already airborne — keep moving
            rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // Gap detected and grounded — stop horizontal movement
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // ── Jump logic ────────────────────────────────────────────────────
        bool cooldownReady = Time.time - lastJumpTime > jumpCooldown;
        bool notAlreadyUp  = rb.linearVelocity.y < 0.1f;

        if (isGrounded && cooldownReady && notAlreadyUp)
        {
            bool playerAbove = distY > jumpThreshold;
            bool playerClose = distX < maxJumpDistance;

            // Case 1: gap ahead — jump to cross it regardless of player height
            if (gapAhead)
            {
                rb.linearVelocity = new Vector2(dirX * moveSpeed, jumpForce);
                lastJumpTime = Time.time;
                Debug.Log("[Matcha] Jumping over gap.");
            }
            // Case 2: player is on a higher platform — jump to reach them
            else if (playerAbove && playerClose)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                lastJumpTime = Time.time;
                Debug.Log("[Matcha] Jumping towards player above.");
            }
        }

        // ── Velocity clamp ────────────────────────────────────────────────
        rb.linearVelocity = new Vector2(
            Mathf.Clamp(rb.linearVelocity.x, -moveSpeed * 2f, moveSpeed * 2f),
            Mathf.Clamp(rb.linearVelocity.y, -20f, jumpForce)
        );
    }

    // ── Ground check ──────────────────────────────────────────────────────

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

    // ── Gap detection ─────────────────────────────────────────────────────
    // Casts a short ray straight DOWN from just ahead of matcha's feet.
    // If no Ground-tagged object is found within gapProbeDepth, it is a gap.

    bool IsGapAhead(float dirX)
    {
        // Origin: slightly ahead of matcha horizontally, at foot level
        Vector2 origin = (Vector2)groundCheck.position
            + new Vector2(dirX * gapProbeDistance, 0f);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, gapProbeDepth);

        bool groundFound = false;
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider.gameObject == gameObject) continue;
            foreach (string tag in groundTags)
            {
                if (h.collider.CompareTag(tag))
                {
                    groundFound = true;
                    break;
                }
            }
            if (groundFound) break;
        }

        // Draw debug ray — green = ground found, red = gap
        Debug.DrawRay(origin, Vector2.down * gapProbeDepth,
            groundFound ? Color.green : Color.red);

        return !groundFound;
    }

    // ── Player bump ───────────────────────────────────────────────────────

    void OnCollisionEnter2D(Collision2D col)
    {
        if ((col.gameObject.CompareTag("Mug") || col.gameObject.CompareTag("Bean"))
            && Time.time - lastBumpTime > bumpCooldown
            && !isBumping)
        {
            lastBumpTime = Time.time;
            StartCoroutine(DoBump(col.gameObject));
        }
    }

    IEnumerator DoBump(GameObject mugObj)
    {
        isBumping = true;

        rb.linearVelocity  = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints     = RigidbodyConstraints2D.FreezeAll;

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

        rb.constraints    = RigidbodyConstraints2D.FreezeRotation;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.3f);
        isBumping = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

            // Show gap probe ray in Scene view even when not playing
            float dirX = player != null
                ? Mathf.Sign(player.position.x - transform.position.x)
                : 1f;
            Vector2 probeOrigin = (Vector2)groundCheck.position
                + new Vector2(dirX * gapProbeDistance, 0f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(probeOrigin, probeOrigin + Vector2.down * gapProbeDepth);
        }
    }
}