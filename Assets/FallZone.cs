using UnityEngine;
using UnityEngine.SceneManagement;

public class FallZone : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Bean"))
        {
            BeanController bean = col.gameObject.GetComponent<BeanController>();
            if (bean != null && bean.IsInGrinder()) return; // ignore if grinding
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}