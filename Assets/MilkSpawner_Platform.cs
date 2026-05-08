using UnityEngine;
using System.Collections;

/// <summary>
/// Place above a platform. Spawns milk drops forever
/// within the platform width. Dead simple — no counting,
/// no tags, just spawns on a timer.
///
/// SETUP:
/// 1. Create Empty → name MilkSpawner_A
/// 2. Position ABOVE Platform A in Scene view
/// 3. Add Component → MilkSpawner_Platform
/// 4. Assign milkDropPrefab (MilkDrop_Spawned prefab)
/// 5. Assign platform (drag Platform A from Hierarchy)
/// </summary>
public class MilkSpawner_Platform : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject milkDropPrefab;

    [Header("Platform")]
    [Tooltip("Drag the platform this spawner is above")]
    public Transform platform;

    [Header("Spawn Settings")]
    [Tooltip("Seconds between each drop")]
    public float spawnInterval = 2.5f;

    [Tooltip("Max drops allowed on platform at once — " +
             "stops spawning if too many pile up")]
    public int maxDropsAtOnce = 4;

    [Tooltip("How far left/right drops can spawn " +
             "(0 = always centre)")]
    public float spawnSpread = 0f;

    private void Start()
    {
        if (milkDropPrefab == null)
        {
            Debug.LogError(
                "[MilkSpawner_Platform] Milk drop prefab " +
                "not assigned on " + gameObject.name +
                "! Attach a prefab in the Inspector.");
            return;
        }

        if (platform == null)
        {
            Debug.LogWarning(
                "[MilkSpawner_Platform] No platform " +
                "assigned on " + gameObject.name +
                ". Drops will spawn at spawner position.");
        }

        StartCoroutine(SpawnForever());
    }

    IEnumerator SpawnForever()
    {
        // Wait one frame for scene to fully load
        yield return null;

        while (true)
        {
            // Count drops currently near this spawner
            int nearby = CountNearbyDrops();

            if (nearby < maxDropsAtOnce)
            {
                SpawnDrop();
                Debug.Log("[MilkSpawner_Platform] " +
                    gameObject.name + " spawned a drop. " +
                    "Nearby: " + nearby);
            }
            else
            {
                Debug.Log("[MilkSpawner_Platform] " +
                    gameObject.name + " skipped — " +
                    nearby + " drops nearby already.");
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnDrop()
    {
        // Calculate X position within platform width
        float halfWidth = GetPlatformHalfWidth();
        float spread = spawnSpread > 0
            ? spawnSpread
            : halfWidth * 0.7f;

        float randomX = Random.Range(-spread, spread);

        float centreX = platform != null
            ? platform.position.x
            : transform.position.x;

        Vector3 spawnPos = new Vector3(
            centreX + randomX,
            transform.position.y,  // spawn at spawner height
            0f);

        Instantiate(milkDropPrefab, spawnPos,
            Quaternion.identity);
    }

    int CountNearbyDrops()
    {
        // Find all MilkDrop_2 components in scene
        // and count ones near this platform
        MilkDrop_2[] allDrops =
            FindObjectsOfType<MilkDrop_2>();

        float centreX = platform != null
            ? platform.position.x
            : transform.position.x;

        float checkRange = GetPlatformHalfWidth() + 2f;
        int count = 0;

        foreach (MilkDrop_2 drop in allDrops)
        {
            if (drop == null) continue;
            if (Mathf.Abs(
                drop.transform.position.x - centreX)
                < checkRange)
                count++;
        }

        return count;
    }

    float GetPlatformHalfWidth()
    {
        if (platform == null) return 2f;

        SpriteRenderer sr =
            platform.GetComponent<SpriteRenderer>();
        if (sr != null)
            return sr.bounds.extents.x;

        return platform.localScale.x * 0.5f;
    }

    void OnDrawGizmosSelected()
    {
        float hw = GetPlatformHalfWidth() * 0.7f;
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.7f);

        Vector3 left = new Vector3(
            (platform != null
                ? platform.position.x
                : transform.position.x) - hw,
            transform.position.y, 0);
        Vector3 right = new Vector3(
            (platform != null
                ? platform.position.x
                : transform.position.x) + hw,
            transform.position.y, 0);

        Gizmos.DrawLine(left, right);
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}