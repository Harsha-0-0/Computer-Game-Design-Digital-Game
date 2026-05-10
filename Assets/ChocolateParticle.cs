using UnityEngine;

public class ChocolateParticle : MonoBehaviour
{
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;

    private Vector3 startPos;

    [Header("Audio")]
    public AudioClip collectSound;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Bob up and down
        float newY = startPos.y +
            Mathf.Sin(Time.time * bobSpeed)
            * bobHeight;
        transform.position = new Vector3(
            startPos.x, newY, startPos.z
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bean") ||
            other.CompareTag("Mug"))
        {
            // Tell LevelManager chocolate collected
            if (LevelManager.Instance != null)
                LevelManager.Instance.ChocolateCollected();

            PlayAndDestroy();
        }
    }

    void PlayAndDestroy()
    {
        if (collectSound != null)
        {
            GameObject soundHost = new GameObject("BeanCollectSound");
            soundHost.transform.position = transform.position;
            AudioSource tempAudio = soundHost.AddComponent<AudioSource>();
            tempAudio.PlayOneShot(collectSound);
            Destroy(soundHost, collectSound.length);
        }

        Destroy(gameObject);
    }
}