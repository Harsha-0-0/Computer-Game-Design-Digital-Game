using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    private Vector3 startPos;
    private int direction = 1;
    private Transform beanTransform;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        Vector3 previousPos = transform.position;

        transform.position += new Vector3(moveSpeed * direction * Time.deltaTime, 0, 0);

        float offset = transform.position.x - startPos.x;
        if (offset >= moveDistance) direction = -1;
        if (offset <= -moveDistance) direction = 1;

        // Instead of parenting move the bean manually
        if (beanTransform != null)
        {
            Vector3 delta = transform.position - previousPos;
            beanTransform.position += new Vector3(delta.x, 0, 0);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Bean"))
        {
            // Don't parent just track it
            beanTransform = col.gameObject.transform;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Bean"))
        {
            beanTransform = null;
        }
    }
}