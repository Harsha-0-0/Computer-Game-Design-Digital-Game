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

    [Header("Volume")]
    public Slider volumeSlider;

    private bool isPaused = false;

    void Start()
    {
        // Make sure pause menu is hidden at start
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Set volume slider to current volume
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // Wire up buttons
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);
        if (continueButton != null)
            continueButton.onClick.AddListener(Resume);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
        if (exitToMenuButton != null)
            exitToMenuButton.onClick.AddListener(ExitToMenu);
    }

    void Update()
    {
        // Also allow Escape key to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;  // Freeze the game
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;  // Resume the game
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
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
        // Always restore time scale if this object is destroyed
        Time.timeScale = 1f;
    }
}