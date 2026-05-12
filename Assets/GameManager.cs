using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public int totalLives = 3;
    public float levelTime = 240f;

    private int currentLives;
    private float savedTimer = -1f;
    private bool initialized = false;
    private bool level1Visited = false;

    void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!initialized)
        {
            initialized = true;
            currentLives = totalLives;
            savedTimer = levelTime;






        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    else
    {
        Destroy(gameObject);
    }
}

void OnDestroy()
{
    SceneManager.sceneLoaded -= OnSceneLoaded;
}

void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name == "Level1")
    {
        if (!level1Visited)
        {
            // First time entering Level 1 — reset lives and timer
            level1Visited = true;
            currentLives = totalLives;
            savedTimer = levelTime;
            Debug.Log("[GameManager] Level1 first entry — lives and timer reset.");
        }
        // subsequent loads are restarts — don't reset
    }
    else if (scene.name == "TutorialScene")
    {
        
        // Reset the flag so if player goes back to tutorial
        // and replays, Level 1 resets again on next entry
        
        level1Visited = false;
    }
}

    public void LoseLife()
    {
        currentLives--;
        Debug.Log("Lives left: " + currentLives);

        if (currentLives <= 0)
            ResetGame();
    }

    public void ResetGame()
{
    initialized = false;
    currentLives = totalLives;
    savedTimer = levelTime;
    level1Visited = false;
    SceneManager.LoadScene("Level1");
}

    // Called by LevelManager to save
    // timer before scene reload
    public void SaveTimer(float time)
    {
        savedTimer = time;
    }

    // Called by LevelManager to get
    // saved timer on scene start
    public float GetSavedTimer()
    {
        return savedTimer;
    }

    // Reset timer for new level
    public void ResetTimer()
    {
        savedTimer = levelTime;
    }

    public int GetLives()
    {
        return currentLives;
    }

    public void SetLives(int lives)
    {
        currentLives = lives;
    }
}