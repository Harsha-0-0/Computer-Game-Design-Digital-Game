using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int totalBeans = 5;
    private int collectedBeans = 0;

    void Awake()
    {
        Instance = this;
    }

    public void BeanCollected()
    {
        collectedBeans++;
        Debug.Log("Beans collected: " + collectedBeans + "/" + totalBeans);

        if (collectedBeans >= totalBeans)
        {
            Debug.Log("All beans collected! Door is unlocked!");
            LevelDoor door = FindFirstObjectByType<LevelDoor>();
            if (door != null) door.Unlock();
        }
    }

    public int GetCollected() { return collectedBeans; }
    public int GetTotal() { return totalBeans; }
}