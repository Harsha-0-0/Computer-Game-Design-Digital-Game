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
                () => LoadLevel("Level_2")
            );

        // Level 3 now clickable!
        if (level3Button != null)
            level3Button.onClick.AddListener(
                () => LoadLevel("Level 3")
            );

        // Level 4 now clickable!
        if (level4Button != null)
            level4Button.onClick.AddListener(
                () => LoadLevel("Level 4")
            );
    }

    void LoadLevel(string sceneName)
    {
        if (UISoundManager.Instance != null)
        UISoundManager.Instance.PlayClick();
        SceneManager.LoadScene(sceneName);
    }

    public void GoBack()
    {
        if (UISoundManager.Instance != null)
        UISoundManager.Instance.PlayClick();
        SceneManager.LoadScene("MugSelectionScene");
    }
}