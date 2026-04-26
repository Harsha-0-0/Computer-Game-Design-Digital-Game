using UnityEngine;
using TMPro;

public class FrothManager : MonoBehaviour
{
    public static FrothManager Instance;

    [Header("Foam Settings")]
    public int totalFoamToWin   = 20;
    public int totalFoamInLevel = 24;

    [Header("UI (optional)")]
    public TextMeshProUGUI foamCounterUI;

    private int collectedFoam = 0;

    void Awake()
    {
        Instance = this;
    }

    public void FoamCollected()
    {
        collectedFoam++;
        Debug.Log("Foam collected: " + collectedFoam + "/" + totalFoamToWin);

        UpdateUI();

        if (collectedFoam >= totalFoamToWin)
        {
            Debug.Log("Enough foam! Door is unlocked!");
            LevelDoor door = FindFirstObjectByType<LevelDoor>();
            if (door != null) door.Unlock();
        }
    }

    void UpdateUI()
    {
        if (foamCounterUI != null)
            foamCounterUI.text = "Foam: " + collectedFoam + " / " + totalFoamToWin;
    }

    public int GetCollected() { return collectedFoam; }
    public int GetTotal()     { return totalFoamToWin; }
}