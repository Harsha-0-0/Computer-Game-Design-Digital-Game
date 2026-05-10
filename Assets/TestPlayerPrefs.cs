using UnityEngine;

public class TestPlayerPrefs : MonoBehaviour
{
    [Header("Test Values")]
    public float testTotalTime   = 185f;
    public float testPersonalBest = 150f;
    public int   testSelectedMug  = 0;

    [ContextMenu("Set Test PlayerPrefs")]
    public void SetTestPrefs()
    {
        PlayerPrefs.SetFloat("TotalTime",     testTotalTime);
        PlayerPrefs.SetFloat("PersonalBest",  testPersonalBest);
        PlayerPrefs.SetInt("SelectedMug",     testSelectedMug);
        PlayerPrefs.Save();
        Debug.Log("Test PlayerPrefs set!");
    }

    [ContextMenu("Clear All PlayerPrefs")]
    public void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs cleared!");
    }
}