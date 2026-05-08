using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks total time across all levels.
/// Attach to your GameManager or a persistent GameObject.
/// Time accumulates across scenes and saves to PlayerPrefs as "TotalTime".
///
/// SETUP IN UNITY:
///  1. Attach to GameManager (or any persistent object with DontDestroyOnLoad)
///  2. Call GameTimer.Instance.StartTimer() when the game begins (Level 1 start)
///  3. Call GameTimer.Instance.StopTimer() when the player reaches the end scene
///  4. The EndScene.cs reads "TotalTime" from PlayerPrefs automatically
/// </summary>
public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    private float totalTime  = 0f;
    private bool  isRunning  = false;

    // Scenes where the timer should NOT run (menus, selection screens, end screen)
    [Tooltip("Scene names where the timer pauses automatically")]
    public string[] pausedScenes = {
        "TitleScreen", "MugSelectionScene", "LevelSelectScene",
        "TutorialScene", "PremiseScene", "EndScene"
    };

    private const string KEY_TOTAL_TIME = "TotalTime";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Resume any previously saved time (in case of scene reload mid-run)
        totalTime = PlayerPrefs.GetFloat(KEY_TOTAL_TIME, 0f);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (!isRunning) return;
        totalTime += Time.deltaTime;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Scene handling — auto pause/resume based on scene name
    // ─────────────────────────────────────────────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldPause = System.Array.Exists(
            pausedScenes,
            s => s == scene.name
        );

        if (shouldPause)
        {
            PauseTimer();

            // If end scene loaded, save final time
            if (scene.name == "EndScene")
                SaveTime();
        }
        else
        {
            ResumeTimer();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Call at the very start of a new game run (e.g. from Level 1 Start).
    /// Resets accumulated time to zero.
    /// </summary>
    public void StartFreshTimer()
    {
        totalTime = 0f;
        isRunning = true;
        PlayerPrefs.DeleteKey(KEY_TOTAL_TIME);
        PlayerPrefs.Save();
        Debug.Log("[GameTimer] Timer started fresh.");
    }

    public void PauseTimer()
    {
        isRunning = false;
        SaveTime();
    }

    public void ResumeTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
        SaveTime();
        Debug.Log($"[GameTimer] Final time: {FormatTime(totalTime)}");
    }

    public void SaveTime()
    {
        PlayerPrefs.SetFloat(KEY_TOTAL_TIME, totalTime);
        PlayerPrefs.Save();
    }

    public float TotalTime    => totalTime;
    public string FormattedTime => FormatTime(totalTime);

    private string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{mins:00}:{secs:00}";
    }
}