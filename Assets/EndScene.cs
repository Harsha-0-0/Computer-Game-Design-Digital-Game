using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the end screen:
///  - Displays total game completion time
///  - Tracks and displays personal best (least time)
///  - Shows the mug sprite the player selected
///
/// SETUP IN UNITY:
///  1. Attach this script to a GameObject in your end scene
///  2. Assign all UI references in the Inspector
///  3. Make sure GameTimer.cs is saving "TotalTime" to PlayerPrefs when level completes
/// </summary>
public class EndScene : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text showing 'Order delivered in: MM:SS'")]
    public TMP_Text completionTimeText;

    [Tooltip("Text showing 'Personal record: MM:SS'")]
    public TMP_Text personalRecordText;

    [Tooltip("The mug Image/SpriteRenderer on screen — assign whichever your scene uses")]
    public SpriteRenderer mugSpriteRenderer; // if mug is a sprite in world space
    public Image mugImage;                   // if mug is a UI Image

    [Header("Mug Sprites")]
    [Tooltip("Same order as MugSelectionManager — index must match SelectedMug PlayerPref")]
    public Sprite[] mugSprites;

    [Header("Buttons (optional)")]
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Scenes")]
    public string mainMenuScene  = "TitleScreen";
    public string firstLevelScene = "Level_1";

    // PlayerPrefs keys
    private const string KEY_TOTAL_TIME     = "TotalTime";
    private const string KEY_PERSONAL_BEST  = "PersonalBest";
    private const string KEY_SELECTED_MUG   = "SelectedMug";

    void Start()
    {
        ShowCompletionTime();
        ShowPersonalRecord();
        ShowSelectedMug();

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(PlayAgain);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Time display
    // ─────────────────────────────────────────────────────────────────────────

    private void ShowCompletionTime()
    {
        float totalTime = GetTotalTime();

        if (completionTimeText != null)
            completionTimeText.text = FormatTime(totalTime);

        Debug.Log("[EndScene] Completion time: " + FormatTime(totalTime));
    }

    private float GetTotalTime()
    {
        // Try GameTimer first (if attached to GameManager)
        float timerTime = PlayerPrefs.GetFloat(KEY_TOTAL_TIME, 0f);
        if (timerTime > 0f) return timerTime;

        // Fallback: calculate from GameManager's saved timer
        // savedTimer starts at levelTime (240) and counts down,
        // so time taken = levelTime - savedTimer... but since
        // it persists across levels we use what GameManager has
        if (GameManager.Instance != null)
        {
            // Each level is 240s max, use remaining timer to estimate
            float remaining = GameManager.Instance.GetSavedTimer();
            float levelTime = GameManager.Instance.levelTime;
            float elapsed   = levelTime - remaining;
            if (elapsed > 0f) return elapsed;
        }

        return 0f;
    }

    private void ShowPersonalRecord()
    {
        float totalTime    = GetTotalTime();
        float personalBest = PlayerPrefs.GetFloat(KEY_PERSONAL_BEST, float.MaxValue);

        // Update personal best if this run was faster (lower time = better)
        if (totalTime > 0f && totalTime < personalBest)
        {
            personalBest = totalTime;
            PlayerPrefs.SetFloat(KEY_PERSONAL_BEST, personalBest);
            PlayerPrefs.Save();
            Debug.Log($"[EndScene] New personal best: {FormatTime(personalBest)}");
        }

        if (personalRecordText != null)
        {
            // If no valid best yet, show current time as the first record
            float displayTime = personalBest == float.MaxValue ? totalTime : personalBest;
            personalRecordText.text = FormatTime(displayTime);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Mug display
    // ─────────────────────────────────────────────────────────────────────────

    private void ShowSelectedMug()
    {
        int selectedMug = PlayerPrefs.GetInt(KEY_SELECTED_MUG, 0);

        // Use Inspector sprites directly — these are the cappuccino-filled versions
        // NOT Resources/MugSprites which are the empty gameplay mugs
        if (mugSprites == null || mugSprites.Length == 0)
        {
            Debug.LogWarning("[EndScene] No mug sprites assigned in Inspector!");
            return;
        }

        // Clamp in case index is out of range
        int index = Mathf.Clamp(selectedMug, 0, mugSprites.Length - 1);
        Sprite mugSprite = mugSprites[index];

        if (mugSprite == null)
        {
            Debug.LogWarning("[EndScene] Mug sprite at index " + index + " is null!");
            return;
        }

        if (mugSpriteRenderer != null)
            mugSpriteRenderer.sprite = mugSprite;

        if (mugImage != null)
            mugImage.sprite = mugSprite;

        Debug.Log("[EndScene] Showing mug index: " + index + " sprite: " + mugSprite.name);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Buttons
    // ─────────────────────────────────────────────────────────────────────────

    public void PlayAgain()
    {
        // Clear the saved total time so next run starts fresh
        PlayerPrefs.DeleteKey(KEY_TOTAL_TIME);
        PlayerPrefs.Save();
        SceneManager.LoadScene(firstLevelScene);
    }

    public void GoToMainMenu()
    {
        PlayerPrefs.DeleteKey(KEY_TOTAL_TIME);
        PlayerPrefs.Save();
        SceneManager.LoadScene(mainMenuScene);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Converts seconds to MM:SS display format.
    /// </summary>
    private string FormatTime(float seconds)
    {
        if (seconds <= 0f || seconds == float.MaxValue)
            return "00:00";

        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{mins:00}:{secs:00}";
    }
}