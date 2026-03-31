using UnityEngine;
using System.Collections;


public class GrinderGoal : MonoBehaviour
{
    [Header("Grinder Settings")]
    public float spinDuration = 2.5f;       // How long the bean spins
    public float spinSpeed = 720f;           // Degrees per second during grind

    [Header("Powder Particle Settings")]
    public GameObject powderParticlePrefab; // Assign a small brown circle prefab
    public int powderCount = 20;
    public float powderSpread = 0.4f;       // How wide particles spray sideways
    public float powderFallSpeed = 2f;

    [Header("Cup Reference")]
    public Transform cupTarget;             // Assign the cup object's transform

    [Header("Win Screen (optional)")]
    public GameObject winCanvas;            // Assign a UI canvas for win state

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bean") && !triggered)
        {
            triggered = true;
            BeanController bean = other.GetComponent<BeanController>();
            if (bean != null) bean.EnterGrinder();
            StartCoroutine(GrindSequence(other.gameObject));
        }
    }

    IEnumerator GrindSequence(GameObject bean)
    {
        float elapsed = 0f;

        // Spin the bean in place
        while (elapsed < spinDuration)
        {
            bean.transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Hide the bean (grinding is "done")
        Vector3 grindPosition = bean.transform.position;
        bean.SetActive(false);

        // Spawn powder particles
        for (int i = 0; i < powderCount; i++)
        {
            if (powderParticlePrefab == null) break;

            GameObject particle = Instantiate(
                powderParticlePrefab,
                grindPosition,
                Quaternion.identity
            );

            // Give each particle a slightly different sideways drift + downward fall
            Rigidbody2D prb = particle.GetComponent<Rigidbody2D>();
            if (prb != null)
            {
                float randomX = Random.Range(-powderSpread, powderSpread);
                prb.linearVelocity = new Vector2(randomX, -powderFallSpeed);
            }

            yield return new WaitForSeconds(0.05f); // Stagger the spray slightly
        }

        // Wait for particles to settle into the cup
        yield return new WaitForSeconds(1.5f);

        // Show win screen or load next scene
        if (winCanvas != null)
        {
            winCanvas.SetActive(true);
        }
        else
        {
            Debug.Log("Bean ground into powder — coffee is ready! Load next scene here.");
        }
    }
}