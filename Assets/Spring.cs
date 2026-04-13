using UnityEngine;

public class Spring : MonoBehaviour
{
    public float springForce = 12f;
    public float squishDuration = 0.15f;

    private Vector3 originalScale;
    private bool isSquishing = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Bean"))
        {
            Rigidbody2D rb = col.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Cancel any downward velocity first then apply spring force
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(new Vector2(0f, springForce), ForceMode2D.Impulse);
            }

            if (!isSquishing)
                StartCoroutine(SquishAnimation());
        }
    }

    System.Collections.IEnumerator SquishAnimation()
    {
        isSquishing = true;

        // Squish down
        transform.localScale = new Vector3(
            originalScale.x * 1.3f,
            originalScale.y * 0.5f,
            originalScale.z
        );

        yield return new WaitForSeconds(squishDuration);

        // Spring back
        transform.localScale = originalScale;
        isSquishing = false;
    }
}