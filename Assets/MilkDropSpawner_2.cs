using System.Collections;
using UnityEngine;

/// <summary>
/// Fixed-position milk drop spawner for Level 2 Zone 2.
/// Place two of these in your scene. Together they spawn MORE drops than the
/// 40 required — the player must use the slippery platform to shed any excess
/// before reaching the end croissant.
///
/// SETUP IN UNITY:
/// 1. Create an empty GameObject, name it "MilkSpawner_Zone2_A" (and B for second)
/// 2. Attach this script
/// 3. Assign milkDropPrefab (your MilkDrop_2 prefab)
/// 4. Set dropsToSpawn on each — default is 25 per spawner (50 total vs 40 needed)
/// 5. Adjust spawnInterval and spawnAreaWidth to taste
/// </summary>
public class MilkDropSpawner_2 : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Drag your MilkDrop_2 prefab here")]
    public GameObject milkDropPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Total drops this spawner produces. Set ABOVE 20 so the two spawners " +
             "together produce more than the 40 required — excess must be spilled " +
             "on the slippery platform.")]
    public int dropsToSpawn = 25;

    [Tooltip("Seconds between each drop. Lower = faster rain.")]
    public float spawnInterval = 1.0f;

    [Tooltip("Horizontal spread around this spawner's position (0 = no spread)")]
    public float spawnAreaWidth = 2f;

    [Tooltip("Should spawning begin automatically on Start?")]
    public bool autoStart = true;

    [Tooltip("Delay before the first drop spawns (seconds)")]
    public float initialDelay = 0f;

    [Header("Zone Trigger (Optional)")]
    [Tooltip("If set, spawning only begins when the player enters this collider. " +
             "Leave null to use autoStart instead.")]
    public Collider2D triggerZone;

    // Internal state
    private int dropsSpawned = 0;
    private bool spawning = false;
    private Coroutine spawnCoroutine;

    void Start()
    {
        if (milkDropPrefab == null)
        {
            Debug.LogError($"[MilkDropSpawner_2] No milkDropPrefab assigned on {gameObject.name}!");
            return;
        }

        if (autoStart && triggerZone == null)
            StartSpawning();
    }

    /// <summary>
    /// Begin spawning. Safe to call multiple times — ignores if already spawning.
    /// </summary>
    public void StartSpawning()
    {
        if (spawning) return;
        spawning = true;
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// Stop spawning immediately. Already-spawned drops remain in the scene.
    /// </summary>
    public void StopSpawning()
    {
        spawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (dropsSpawned < dropsToSpawn)
        {
            SpawnOneDrop();
            dropsSpawned++;

            if (dropsSpawned < dropsToSpawn)
                yield return new WaitForSeconds(spawnInterval);
        }

        spawning = false;
        Debug.Log($"[MilkDropSpawner_2] {gameObject.name} finished — spawned {dropsToSpawn} drops.");
    }

    private void SpawnOneDrop()
    {
        float offsetX = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);
        Vector3 spawnPos = transform.position + new Vector3(offsetX, 0f, 0f);

        GameObject drop = Instantiate(milkDropPrefab, spawnPos, Quaternion.identity);

        MilkDrop_2 milkDrop = drop.GetComponent<MilkDrop_2>();
        if (milkDrop != null)
            milkDrop.isSpawned = true;
    }

    // ── Zone trigger ──────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerZone != null && other.CompareTag("Player"))
            StartSpawning();
    }

    // ── Editor gizmo ──────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.8f);
        Vector3 left  = transform.position + Vector3.left  * (spawnAreaWidth / 2f);
        Vector3 right = transform.position + Vector3.right * (spawnAreaWidth / 2f);
        Gizmos.DrawLine(left, right);
        Gizmos.DrawSphere(transform.position, 0.12f);
    }

    public int  DropsSpawnedSoFar => dropsSpawned;
    public bool IsFinished        => dropsSpawned >= dropsToSpawn && !spawning;
}