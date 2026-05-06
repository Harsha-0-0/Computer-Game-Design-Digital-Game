using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SteamZone : MonoBehaviour
{
    [Header("Steam Settings")]
    public float launchForce = 12f;
    public float steamDuration = 0.5f;

    [Header("Visual")]
    public GameObject steamPuffPrefab;
    public int maxPuffs = 6;
    public float puffSpawnInterval = 0.3f;
    public float puffRiseSpeed = 2f;
    public float puffLifetime = 1.5f;

    private bool isSteaming = false;
    private List<GameObject> activePuffs = 
        new List<GameObject>();

    void Start()
    {
        // Continuously spawn steam puffs
        StartCoroutine(SpawnSteamPuffs());
    }

    IEnumerator SpawnSteamPuffs()
    {
        while (true)
        {
            if (steamPuffPrefab != null)
            {
                // Find the SteamTrigger child position
                Transform triggerPos = transform.Find("SteamTrigger");
                Vector3 origin = triggerPos != null 
                    ? triggerPos.position 
                    : transform.position;

                Vector3 spawnPos = origin + new Vector3(
                    Random.Range(-0.2f, 0.2f),
                    0f,
                    0
                );

                GameObject puff = Instantiate(
                    steamPuffPrefab,
                    spawnPos,
                    Quaternion.identity
                );

                activePuffs.Add(puff);
                StartCoroutine(AnimatePuff(puff));

                if (activePuffs.Count > maxPuffs)
                {
                    GameObject old = activePuffs[0];
                    activePuffs.RemoveAt(0);
                    if (old != null) Destroy(old);
                }
            }

            yield return new WaitForSeconds(puffSpawnInterval);
        }
    }

    IEnumerator AnimatePuff(GameObject puff)
    {
        if (puff == null) yield break;

        float elapsed = 0f;
        Vector3 startPos = puff.transform.position;
        Vector3 startScale = puff.transform.localScale;
        SpriteRenderer sr = 
            puff.GetComponent<SpriteRenderer>();

        while (elapsed < puffLifetime && puff != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / puffLifetime;

            // Rise upward
            puff.transform.position = startPos +
                new Vector3(
                    Mathf.Sin(elapsed * 2f) * 0.1f,
                    elapsed * puffRiseSpeed,   // always rises up
                    0
            );

            // Grow bigger as it rises
            puff.transform.localScale = Vector3.Lerp(
                startScale,
                startScale * 2f,
                t
            );

            // Fade out as it rises
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(0.7f, 0f, t);
                sr.color = c;
            }

            yield return null;
        }

        if (puff != null)
        {
            activePuffs.Remove(puff);
            Destroy(puff);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mug") && !isSteaming)
        {
            StartCoroutine(LaunchMug(other));
        }
    }

    IEnumerator LaunchMug(Collider2D mug)
    {
        isSteaming = true;

        Rigidbody2D rb = 
            mug.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x, 0f
            );
            rb.AddForce(
                new Vector2(0f, launchForce),
                ForceMode2D.Impulse
            );
        }

        yield return new WaitForSeconds(steamDuration);
        isSteaming = false;
    }
}