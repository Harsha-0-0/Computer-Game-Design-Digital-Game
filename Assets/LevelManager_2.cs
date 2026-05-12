using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages all Level 2 logic:
///  - Tracks milk drop count (target: exactly 40)
///  - Spawners produce MORE than 40 — player must spill excess on slippery platform
///  - Slippery platform is a TOOL: standing on it for 3s spills 2 drops
///  - Door only unlocks at exactly 40 — over or under is locked
///  - Shows UI feedback for spills and over/under count state
///
/// SETUP IN UNITY:
///  1. Create a GameObject "LevelManager_2", attach this script
///  2. Assign all UI references in the Inspector
///  3. Assign levelDoor (the croissant GameObject)
///  4. Leave targetDropCount at 40
/// </summary>
public class LevelManager_2 : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // Inspector fields
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Timer")]
    public float levelTime = 240f;

    [Header("Drop Count")]
    [Tooltip("Exact number of milk drops needed to unlock the door")]
    public int targetDropCount = 40;

    [Header("UI References")]
    [Tooltip("Text showing current / target drop count")]
    public TMP_Text milkDropCountText;

    [Tooltip("Panel shown when slippery platform spills drops")]
    public GameObject spillWarningPanel;

    [Tooltip("Text inside the spill warning panel")]
    public TMP_Text spillWarningText;

    [Tooltip("Duration (seconds) the spill warning stays visible")]
    public float spillWarningDuration = 2f;

    [Tooltip("Floating '-2' text prefab that appears near the mug on spill")]
    public GameObject floatingMinusTextPrefab;

    [Tooltip("Optional: panel or text shown when player has too many drops")]
    public GameObject overCountWarningPanel;

    [Tooltip("Text inside the over-count warning panel")]
    public TMP_Text overCountWarningText;

    [Header("Door / Croissant")]
    [Tooltip("The croissant GameObject that acts as the end-line door")]
    public GameObject levelDoor;

    [Header("Slippery Platform")]
    [Tooltip("Seconds the mug must stand on a slippery platform before losing drops")]
    public float slipperyTimeThreshold = 1f;

    [Tooltip("Number of milk drops lost per slippery penalty")]
    public int slipperyPenaltyAmount = 1;

    [Tooltip("Cooldown after a penalty before another can trigger (prevents rapid loss)")]
    public float slipperyCooldown = 0.3f;

    [Header("Scene")]
    public string nextSceneName = "Level_3";
    public string failSceneName = "Level2";

    // ─────────────────────────────────────────────────────────────────────────
    // Private state
    // ─────────────────────────────────────────────────────────────────────────

    private int currentDropCount = 0;
    private float timeRemaining;
    private bool levelActive = true;
    private bool onSlipperyPlatform = false;
    private bool slipperyCooldownActive = false;
    private Transform mugTransform;
    private Coroutine spillWarningCoroutine;
    private Coroutine overCountCoroutine;

    // ─────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        // Restore saved timer from GameManager if available
        if (GameManager.Instance != null)
        {
            float saved = GameManager.Instance.GetSavedTimer();
            timeRemaining = saved > 0 ? saved : levelTime;
        }
        else
        {
            timeRemaining = levelTime;
        }

        currentDropCount = 0;
        levelActive = true;

        UpdateDropCountUI();
        UpdateDoorVisual();

        if (spillWarningPanel != null)
            spillWarningPanel.SetActive(false);
        if (overCountWarningPanel != null)
            overCountWarningPanel.SetActive(false);

        // Find mug by tag
        GameObject mug = GameObject.FindGameObjectWithTag("Mug");
        if (mug != null)
            mugTransform = mug.transform;

        // Initialise lives UI
        if (UIManager.Instance != null &&
            GameManager.Instance != null)
            UIManager.Instance.UpdateLives(
                GameManager.Instance.GetLives());
    }

    void Update()
    {
        if (!levelActive) return;

        // Count down timer
        timeRemaining -= Time.deltaTime;
        if (timeRemaining < 0) timeRemaining = 0;

        // Update thermometer
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateTimer(timeRemaining);

        // Timer ran out — treat as falling off
        if (timeRemaining <= 0)
        {
            levelActive = false;
            TriggerFail();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public API — called by MilkDrop_2, SlipperyPlatform, DoorToNextLevel
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Call when a milk drop is collected. No cap — can exceed 40.
    /// </summary>
    public void AddMilkDrop()
    {
        currentDropCount++;
        UpdateDropCountUI();
        UpdateDoorVisual();
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateMilkWithColor(currentDropCount, targetDropCount);

        // Warn player if they've gone over target
        if (currentDropCount > targetDropCount)
            ShowOverCountWarning();
    }

    /// <summary>
    /// Call to deduct drops (slippery platform penalty).
    /// </summary>
    public void RemoveMilkDrops(int amount)
    {
        currentDropCount = Mathf.Max(0,
            currentDropCount - amount);
        UpdateDropCountUI();
        UpdateDoorVisual();
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateMilkWithColor(currentDropCount, targetDropCount);
    }

    /// <summary>
    /// Called by SlipperyPlatform.cs on mug enter.
    /// </summary>
    public void OnEnterSlipperyPlatform()
    {
        onSlipperyPlatform = true;
    }

    /// <summary>
    /// Called by SlipperyPlatform.cs on mug exit.
    /// </summary>
    public void OnExitSlipperyPlatform()
    {
        onSlipperyPlatform = false;
    }

    /// <summary>
    /// Called by SlipperyPlatform.cs once threshold is reached.
    /// </summary>
    public void OnSlipperyPenaltyTriggered()
    {
        if (slipperyCooldownActive) return;
        ApplySlipperyPenalty();
        StartCoroutine(SlipperyCooldownRoutine());
    }

    /// <summary>
    /// Called by DoorToNextLevel — returns true only at exactly 40.
    /// </summary>
    public bool IsExactTarget() =>
        currentDropCount == targetDropCount;

    /// <summary>
    /// Human-readable status for the locked door message.
    /// </summary>
    public string GetDoorLockedReason()
    {
        if (currentDropCount < targetDropCount)
            return $"Need exactly {targetDropCount} milk " +
                   $"drops! ({currentDropCount}/{targetDropCount})";
        if (currentDropCount > targetDropCount)
            return $"Too many milk drops! Use the slippery " +
                   $"platform to spill some. " +
                   $"({currentDropCount}/{targetDropCount})";
        return "";
    }

    /// <summary>
    /// Call from FallZone when mug falls off screen.
    /// </summary>
    public void OnMugFell() => TriggerFail();

    // ─────────────────────────────────────────────────────────────────────────
    // Slippery platform penalty
    // ─────────────────────────────────────────────────────────────────────────

    private void ApplySlipperyPenalty()
    {
        if (currentDropCount <= 0) return;

        int actualLoss = Mathf.Min(
            slipperyPenaltyAmount, currentDropCount);
        RemoveMilkDrops(actualLoss);

        ShowSpillWarning(actualLoss);
        SpawnFloatingMinusText(actualLoss);
    }

    private IEnumerator SlipperyCooldownRoutine()
    {
        slipperyCooldownActive = true;
        yield return new WaitForSeconds(slipperyCooldown);
        slipperyCooldownActive = false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI helpers
    // ─────────────────────────────────────────────────────────────────────────

    private void UpdateDropCountUI()
    {
        if (milkDropCountText == null) return;

        if (currentDropCount < targetDropCount)
        {
            milkDropCountText.text = $"{currentDropCount} / {targetDropCount} Milk Drops";
        }
        else if (currentDropCount == targetDropCount)
        {
            milkDropCountText.text =
                $"Milk Drops: {currentDropCount} / {targetDropCount} ✓";
            milkDropCountText.color = Color.green;
        }
        else
        {
            int excess = currentDropCount - targetDropCount;
            milkDropCountText.text =
                $"Milk Drops: {currentDropCount} / {targetDropCount}" +
                $"  (+{excess} too many!)";
            milkDropCountText.color = Color.red;
        }
    }

    private void UpdateDoorVisual()
    {
        if (levelDoor == null) return;
        SpriteRenderer sr =
            levelDoor.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        sr.color = IsExactTarget()
            ? Color.white
            : new Color(0.5f, 0.5f, 0.5f);
    }

    private void ShowSpillWarning(int amount)
    {
        if (spillWarningPanel == null) return;
        if (spillWarningCoroutine != null)
            StopCoroutine(spillWarningCoroutine);
        spillWarningCoroutine =
            StartCoroutine(SpillWarningRoutine(amount));
    }

    private IEnumerator SpillWarningRoutine(int amount)
    {
        spillWarningPanel.SetActive(true);
        if (spillWarningText != null)
            spillWarningText.text =
                $"-{amount} Milk Drops Spilled!";
        yield return new WaitForSeconds(spillWarningDuration);
        spillWarningPanel.SetActive(false);
    }

    private void ShowOverCountWarning()
    {
        if (overCountWarningPanel == null) return;
        if (overCountCoroutine != null)
            StopCoroutine(overCountCoroutine);
        overCountCoroutine =
            StartCoroutine(OverCountRoutine());
    }

    private IEnumerator OverCountRoutine()
    {
        overCountWarningPanel.SetActive(true);
        if (overCountWarningText != null)
            overCountWarningText.text =
                "Too many drops! Use the slippery " +
                "platform to spill some.";
        yield return new WaitForSeconds(2.5f);
        overCountWarningPanel.SetActive(false);
    }

    private void SpawnFloatingMinusText(int amount)
    {
        if (floatingMinusTextPrefab == null ||
            mugTransform == null) return;

        Vector3 spawnPos =
            mugTransform.position + Vector3.up * 1f;
        GameObject floater = Instantiate(
            floatingMinusTextPrefab,
            spawnPos,
            Quaternion.identity);

        TMP_Text txt =
            floater.GetComponentInChildren<TMP_Text>();
        if (txt != null) txt.text = $"-{amount}";
        Destroy(floater, 1.5f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Ice Pack Hit
    // ─────────────────────────────────────────────────────────────────────────
    public void ReduceTime(float seconds)
    {
        timeRemaining -= seconds;
        if (timeRemaining < 0) timeRemaining = 0;

        // Flash the thermometer
        if (UIManager.Instance != null)
            StartCoroutine(UIManager.Instance.FlashTimer());

        Debug.Log("[LevelManager_2] Time reduced by " +
            seconds + "°. Remaining: " + timeRemaining);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Win / Fail
    // ─────────────────────────────────────────────────────────────────────────

    private void TriggerFail()
    {
        if (!levelActive) return;
        levelActive = false;

        Debug.Log("[LevelManager_2] Mug fell!");

        if (GameManager.Instance != null)
        {
            // Save timer before restarting
            GameManager.Instance.SaveTimer(timeRemaining);
            GameManager.Instance.LoseLife();

            // Update life icons
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateLives(
                    GameManager.Instance.GetLives());

            if (GameManager.Instance.GetLives() > 0)
                StartCoroutine(RestartLevel());
            // If lives = 0, GameManager.ResetGame()
            // handles going back to Level1
        }
        else
        {
            StartCoroutine(RestartLevel());
        }
    }

    private void TriggerWin()
    {
        levelActive = false;
        Debug.Log("[LevelManager_2] Level 2 complete!");

        if (GameManager.Instance != null)
            GameManager.Instance.ResetTimer();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowLevelComplete();
    }

    private IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Read-only state
    // ─────────────────────────────────────────────────────────────────────────

    public int CurrentDropCount => currentDropCount;
    public int TargetDropCount => targetDropCount;
    public bool TargetReached => currentDropCount == targetDropCount;
    public bool OnSlippery => onSlipperyPlatform;
    public float GetTime() => timeRemaining;
}