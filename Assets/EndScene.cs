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
        float totalTime = PlayerPrefs.GetFloat(KEY_TOTAL_TIME, 0f);

        if (completionTimeText != null)
            completionTimeText.text = FormatTime(totalTime);
    }

    private void ShowPersonalRecord()
    {
        float totalTime    = PlayerPrefs.GetFloat(KEY_TOTAL_TIME, 0f);
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

        // Try loading from Resources first (same as MugController)
        Sprite[] resourceMugs = Resources.LoadAll<Sprite>("MugSprites");

        Sprite mugSprite = null;

        if (resourceMugs != null && selectedMug < resourceMugs.Length)
            mugSprite = resourceMugs[selectedMug];
        else if (mugSprites != null && selectedMug < mugSprites.Length)
            mugSprite = mugSprites[selectedMug]; // fallback to Inspector sprites

        if (mugSprite == null)
        {
            Debug.LogWarning("[EndScene] Could not find mug sprite for index: " + selectedMug);
            return;
        }

        // Apply to whichever component exists in the scene
        if (mugSpriteRenderer != null)
            mugSpriteRenderer.sprite = mugSprite;

        if (mugImage != null)
            mugImage.sprite = mugSprite;
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