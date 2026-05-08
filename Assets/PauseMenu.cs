using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu Panel")]
    public GameObject pauseMenuPanel;

    [Header("Buttons")]
    public Button pauseButton;
    public Button continueButton;
    public Button restartButton;
    public Button exitToMenuButton;
    public Button switchMugsButton;

    [Header("Volume")]
    public Slider volumeSlider;

    [Header("Mug Switcher")]
    public string mugSelectionSceneName = "MugSelectionScene";

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (pauseButton      != null) pauseButton.onClick.AddListener(TogglePause);
        if (continueButton   != null) continueButton.onClick.AddListener(Resume);
        if (restartButton    != null) restartButton.onClick.AddListener(RestartLevel);
        if (exitToMenuButton != null) exitToMenuButton.onClick.AddListener(ExitToMenu);
        if (switchMugsButton != null) switchMugsButton.onClick.AddListener(GoToMugSelection);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // ── Mug Selection ─────────────────────────────────────────────────────────

    void GoToMugSelection()
    {
        // Save return scene
        PlayerPrefs.SetString("ReturnToScene", SceneManager.GetActiveScene().name);

        // ── Save Level 2 state ────────────────────────────────────────────
        LevelManager_2 lm2 = FindObjectOfType<LevelManager_2>();
        if (lm2 != null)
        {
            PlayerPrefs.SetInt("SavedMilkDrops", lm2.CurrentDropCount);
            PlayerPrefs.SetInt("HasSavedLevel2State", 1);
        }

        // ── Save mug position (works for any level) ───────────────────────
        GameObject mug = GameObject.FindGameObjectWithTag("Mug");
        if (mug != null)
        {
            PlayerPrefs.SetFloat("SavedMugX", mug.transform.position.x);
            PlayerPrefs.SetFloat("SavedMugY", mug.transform.position.y);
            PlayerPrefs.SetInt("HasSavedMugPosition", 1);
        }

        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(mugSelectionSceneName);
    }

    // ── Pause / Resume ────────────────────────────────────────────────────────

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        // Reset lives to 3 on manual restart
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLives(GameManager.Instance.totalLives);
            GameManager.Instance.ResetTimer(); // reset thermometer to hot side
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelectScene");
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}