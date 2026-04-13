using UnityEngine;

public class CoffeeWindZone : MonoBehaviour
{
    [Header("Wind Settings")]
    public float windForce = 5f;
    public float windOnDuration = 2f;
    public float windOffDuration = 1.5f;

    [Header("Warning Sign")]
    public GameObject warningSign;

    [Header("Wind Visual")]
    public GameObject windCloudPrefab;
    public GameObject windLinePrefab;
    public int windLineCount = 4;

    private bool windActive = false;
    private float timer = 0f;
    private bool beanInside = false;
    private Rigidbody2D beanRb;

    private GameObject windCloud;
    private GameObject[] windLines;

    void Start()
    {
        // Create cloud
        if (windCloudPrefab != null)
        {
            windCloud = Instantiate(
                windCloudPrefab,
                transform.position + new Vector3(-3f, 0.5f, 0),
                Quaternion.identity
            );
            windCloud.SetActive(false);
        }

        // Create wind lines
        if (windLinePrefab != null)
        {
            windLines = new GameObject[windLineCount];
            for (int i = 0; i < windLineCount; i++)
            {
                Vector3 pos = transform.position + new Vector3(
                    -2f + (i * 0.5f),
                    Random.Range(-1f, 1f),
                    0
                );
                windLines[i] = Instantiate(
                    windLinePrefab,
                    pos,
                    Quaternion.identity
                );
                windLines[i].SetActive(false);
            }
        }

        // Show warning sign always — even before bean enters
        if (warningSign != null) warningSign.SetActive(true);
    }

    void Update()
    {
        if (!beanInside)
        {
            // Wind visuals off when bean not inside
            // But warning sign stays ON always
            windActive = false;
            timer = 0f;
            HideWindVisuals();
            return;
        }

        timer += Time.deltaTime;

        if (!windActive && timer >= windOffDuration)
        {
            windActive = true;
            timer = 0f;
            ShowWindVisuals();
        }
        else if (windActive && timer >= windOnDuration)
        {
            windActive = false;
            timer = 0f;
            HideWindVisuals();
        }

        if (windActive && beanRb != null)
        {
            beanRb.AddForce(new Vector2(windForce, 0f));
        }

        // Animate cloud bobbing
        if (windActive && windCloud != null && windCloud.activeSelf)
        {
            float newY = transform.position.y + 0.5f +
                Mathf.Sin(Time.time * 2f) * 0.1f;
            windCloud.transform.position = new Vector3(
                transform.position.x - 3f,
                newY,
                0
            );

            float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.05f;
            windCloud.transform.localScale = new Vector3(
                1.5f * pulse,
                1f * pulse,
                1f
            );
        }

        // Animate wind lines
        if (windActive && windLines != null)
        {
            for (int i = 0; i < windLines.Length; i++)
            {
                if (windLines[i] == null || !windLines[i].activeSelf)
                    continue;

                windLines[i].transform.position += new Vector3(
                    3f * Time.deltaTime, 0, 0
                );

                if (windLines[i].transform.position.x >
                    transform.position.x + 3f)
                {
                    windLines[i].transform.position = new Vector3(
                        transform.position.x - 2.5f,
                        transform.position.y +
                            Random.Range(-1f, 1f),
                        0
                    );
                }
            }
        }
    }

    void ShowWindVisuals()
    {
        if (windCloud != null) windCloud.SetActive(true);
        if (windLines != null)
            foreach (var line in windLines)
                if (line != null) line.SetActive(true);
    }

    void HideWindVisuals()
    {
        if (windCloud != null) windCloud.SetActive(false);
        if (windLines != null)
            foreach (var line in windLines)
                if (line != null) line.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bean"))
        {
            beanInside = true;
            beanRb = other.GetComponent<Rigidbody2D>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bean"))
        {
            beanInside = false;
            beanRb = null;
            windActive = false;
            timer = 0f;
            HideWindVisuals();
        }
    }
}