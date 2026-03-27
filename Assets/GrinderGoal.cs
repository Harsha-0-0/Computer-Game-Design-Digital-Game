using UnityEngine;

public class GrinderGoal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bean"))
        {
            Debug.Log("Bean reached the grinder! Level complete!");
            // Later: load next scene or show a win screen
        }
    }
}