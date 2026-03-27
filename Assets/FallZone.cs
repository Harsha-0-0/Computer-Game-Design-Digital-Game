using UnityEngine;
using UnityEngine.SceneManagement;

public class FallZone : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Bean"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}