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
    public float slipperyTimeThreshold = 3f;

    [Tooltip("Number of milk drops lost per slippery penalty")]
    public int slipperyPenaltyAmount = 2;

    [Tooltip("Cooldown after a penalty before another can trigger (prevents rapid loss)")]
    public float slipperyCooldown = 1.5f;

    [Header("Scene")]
    public string nextSceneName  = "Level3";
    public string failSceneName  = "Level_2";

    // ─────────────────────────────────────────────────────────────────────────
    // Private state
    // ─────────────────────────────────────────────────────────────────────────

    private int  currentDropCount      = 0;
    private bool onSlipperyPlatform    = false;
    private bool slipperyCooldownActive = false;
    private Transform mugTransform;
    private Coroutine spillWarningCoroutine;
    private Coroutine overCountCoroutine;

    // ─────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        currentDropCount = 0;
        UpdateDropCountUI();
        UpdateDoorVisual();

        if (spillWarningPanel    != null) spillWarningPanel.SetActive(false);
        if (overCountWarningPanel != null) overCountWarningPanel.SetActive(false);

        GameObject mug = GameObject.FindGameObjectWithTag("Mug");
        if (mug != null) mugTransform = mug.transform;
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

        // Warn player if they've gone over target
        if (currentDropCount > targetDropCount)
            ShowOverCountWarning();
    }

    /// <summary>
    /// Call to deduct drops (slippery platform penalty).
    /// </summary>
    public void RemoveMilkDrops(int amount)
    {
        currentDropCount = Mathf.Max(0, currentDropCount - amount);
        UpdateDropCountUI();
        UpdateDoorVisual();
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
    /// Spills drops whether the player is over OR under target —
    /// standing on the platform always costs drops.
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
    public bool IsExactTarget() => currentDropCount == targetDropCount;

    /// <summary>
    /// Human-readable status for the locked door message.
    /// </summary>
    public string GetDoorLockedReason()
    {
        if (currentDropCount < targetDropCount)
            return $"Need exactly {targetDropCount} milk drops! ({currentDropCount}/{targetDropCount})";
        if (currentDropCount > targetDropCount)
            return $"Too many milk drops! Use the slippery platform to spill some. ({currentDropCount}/{targetDropCount})";
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

        int actualLoss = Mathf.Min(slipperyPenaltyAmount, currentDropCount);
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
            milkDropCountText.text  = $"Milk Drops: {currentDropCount} / {targetDropCount}";
            milkDropCountText.color = Color.white;
        }
        else if (currentDropCount == targetDropCount)
        {
            milkDropCountText.text  = $"Milk Drops: {currentDropCount} / {targetDropCount} ✓";
            milkDropCountText.color = Color.green;
        }
        else
        {
            int excess = currentDropCount - targetDropCount;
            milkDropCountText.text  = $"Milk Drops: {currentDropCount} / {targetDropCount}  (+{excess} too many!)";
            milkDropCountText.color = Color.red;
        }
    }

    /// <summary>
    /// Greys out the croissant when locked, white when exactly 40.
    /// DoorToNextLevel handles the unlock glow and scene transition.
    /// </summary>
    private void UpdateDoorVisual()
    {
        if (levelDoor == null) return;
        SpriteRenderer sr = levelDoor.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        sr.color = IsExactTarget() ? Color.white : new Color(0.5f, 0.5f, 0.5f);
    }

    private void ShowSpillWarning(int amount)
    {
        if (spillWarningPanel == null) return;
        if (spillWarningCoroutine != null) StopCoroutine(spillWarningCoroutine);
        spillWarningCoroutine = StartCoroutine(SpillWarningRoutine(amount));
    }

    private IEnumerator SpillWarningRoutine(int amount)
    {
        spillWarningPanel.SetActive(true);
        if (spillWarningText != null)
            spillWarningText.text = $"-{amount} Milk Drops Spilled!";
        yield return new WaitForSeconds(spillWarningDuration);
        spillWarningPanel.SetActive(false);
    }

    private void ShowOverCountWarning()
    {
        if (overCountWarningPanel == null) return;
        if (overCountCoroutine != null) StopCoroutine(overCountCoroutine);
        overCountCoroutine = StartCoroutine(OverCountRoutine());
    }

    private IEnumerator OverCountRoutine()
    {
        overCountWarningPanel.SetActive(true);
        if (overCountWarningText != null)
            overCountWarningText.text = "Too many drops! Use the slippery platform to spill some.";
        yield return new WaitForSeconds(2.5f);
        overCountWarningPanel.SetActive(false);
    }

    private void SpawnFloatingMinusText(int amount)
    {
        if (floatingMinusTextPrefab == null || mugTransform == null) return;
        Vector3 spawnPos = mugTransform.position + Vector3.up * 1f;
        GameObject floater = Instantiate(floatingMinusTextPrefab, spawnPos, Quaternion.identity);
        TMP_Text txt = floater.GetComponentInChildren<TMP_Text>();
        if (txt != null) txt.text = $"-{amount}";
        Destroy(floater, 1.5f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Win / Fail
    // ─────────────────────────────────────────────────────────────────────────

    private void TriggerWin()
    {
        // Win sequence handled by DoorToNextLevel (blink + scene load)
        Debug.Log("[LevelManager_2] Level 2 complete!");
    }

    private void TriggerFail()
    {
        Debug.Log("[LevelManager_2] Mug fell!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveTimer(0f);
            GameManager.Instance.LoseLife();

            if (GameManager.Instance.GetLives() > 0)
                StartCoroutine(RestartLevel());
            // If no lives left, GameManager.ResetGame() 
            // handles going back to Level1
        }
        else
        {
            StartCoroutine(RestartLevel());
        }
    }

    private IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Read-only state
    // ─────────────────────────────────────────────────────────────────────────

    public int  CurrentDropCount => currentDropCount;
    public int  TargetDropCount  => targetDropCount;
    public bool TargetReached    => currentDropCount == targetDropCount;
    public bool OnSlippery       => onSlipperyPlatform;
}