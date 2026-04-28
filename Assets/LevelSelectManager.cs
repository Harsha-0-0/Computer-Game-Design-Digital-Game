using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button tutorialButton;
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button;

    [Header("Coming Soon Overlays")]
    public GameObject level4Overlay;

    void Start()
    {
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

        // Level 3 now clickable!
        if (level3Button != null)
            level3Button.onClick.AddListener(
                () => LoadLevel("Level3")
            );

        // Level 4 still coming soon
        if (level4Button != null)
            level4Button.interactable = false;

        // Only Level 4 overlay now
        if (level4Overlay != null)
            level4Overlay.SetActive(true);
    }

    void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}