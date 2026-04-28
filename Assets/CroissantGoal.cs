using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CroissantGoal : MonoBehaviour
{
    [Header("Bob Settings")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;

    [Header("Circle Settings")]
    public float circleRadius = 0.3f;
    public float circleSpeed = 1.5f;

    [Header("Next Scene")]
    public string nextSceneName = "Level1";

    private Vector3 startPos;
    private bool triggered = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (triggered) return;

        // Move in a small circle
        float x = startPos.x + 
            Mathf.Cos(Time.time * circleSpeed) 
            * circleRadius;
        float y = startPos.y + 
            Mathf.Sin(Time.time * circleSpeed) 
            * circleRadius;

        transform.position = new Vector3(x, y, 
            startPos.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mug") && !triggered)
        {
            triggered = true;
            StartCoroutine(LevelComplete());
        }
    }

    IEnumerator LevelComplete()
    {
        // Flash the croissant
        SpriteRenderer sr = 
            GetComponent<SpriteRenderer>();

        for (int i = 0; i < 4; i++)
        {
            if (sr != null) 
                sr.enabled = false;
            yield return new WaitForSeconds(0.1f);
            if (sr != null) 
                sr.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.3f);

        // Load next scene
        SceneManager.LoadScene(nextSceneName);
    }
}