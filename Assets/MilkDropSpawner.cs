using UnityEngine;

public class MilkDropSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject milkDropPrefab;

    public float spawnInterval = 1.5f;

    [Header("Production Limit")]
    public int maxProduction = 20;

    private int currentProduced = 0;

    private float timer = 0f;

    void Update()
    {
        if (milkDropPrefab == null) return;

        // 已达到最大产量，停止生产
        if (currentProduced >= maxProduction)
            return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnMilk();
            timer = 0f;
        }
    }

    void SpawnMilk()
    {
        Instantiate(
            milkDropPrefab,
            transform.position,
            Quaternion.identity
        );

        currentProduced++;

        Debug.Log(
            gameObject.name +
            " Produced: " +
            currentProduced +
            "/" +
            maxProduction
        );
    }
}