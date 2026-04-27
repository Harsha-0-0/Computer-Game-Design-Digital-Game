using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Level Cards")]
    public Button tutorialButton;
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button;

    [Header("Coming Soon Overlays")]
    public GameObject level3Overlay;
    public GameObject level4Overlay;

    void Start()
    {
        // Set up button listeners
        if (tutorialButton != null)
            tutorialButton.onClick.AddListener(
                () => LoadLevel("TutorialScene")
            );

        if (level1Button != null)
            level1Button.onClick.AddListener(
                () => LoadLevel("Level1")
            );

        if (level2Button != null)
            level2Button.onClick.AddListener(
                () => LoadLevel("Level2")
            );

        // Levels 3 and 4 do nothing for now
        if (level3Button != null)
            level3Button.interactable = false;

        if (level4Button != null)
            level4Button.interactable = false;

        // Show coming soon overlays
        if (level3Overlay != null)
            level3Overlay.SetActive(true);
        if (level4Overlay != null)
            level4Overlay.SetActive(true);
    }

    void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}