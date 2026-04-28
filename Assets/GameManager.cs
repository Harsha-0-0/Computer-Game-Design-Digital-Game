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
        }
        else
        {
            Destroy(gameObject);
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