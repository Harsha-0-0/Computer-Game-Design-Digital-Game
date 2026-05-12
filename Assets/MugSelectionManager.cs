using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MugSelectionManager : MonoBehaviour
{
    [Header("Mug Images")]
    public Sprite[] mugSprites;

    [Header("UI References")]
    public Image previewImage;
    public Image[] mugOptions;
    public GameObject[] highlightBorders;
    public TextMeshProUGUI instructionText;

    [Header("Next Scene")]
    public string nextSceneName = "TutorialScene";

    private int selectedMugIndex = 0;

    private bool sceneReady = false;

    void Start()
    {
        // Set up all mug option images
        for (int i = 0; i < mugOptions.Length; i++)
        {
            if (i < mugSprites.Length &&
                mugOptions[i] != null)
            {
                mugOptions[i].sprite = mugSprites[i];
            }
        }

        // Select first mug by default
        SelectMug(0);
        sceneReady = true; // sound only plays after this
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ConfirmSelection();
        }
    }

    public void SelectMug(int index)
    {
        selectedMugIndex = index;

        if (sceneReady && UISoundManager.Instance != null)
        UISoundManager.Instance.PlayClick();

        // Update preview image
        if (previewImage != null &&
            index < mugSprites.Length)
        {
            previewImage.sprite =
                mugSprites[index];
        }

        // Update highlight borders
        for (int i = 0; i < highlightBorders.Length;
            i++)
        {
            if (highlightBorders[i] != null)
                highlightBorders[i].SetActive(
                    i == index
                );
        }
    }

    public void ConfirmSelection()
    {
        if (UISoundManager.Instance != null)
        UISoundManager.Instance.PlayClick();
        PlayerPrefs.SetInt(
            "SelectedMug",
            selectedMugIndex
        );
        PlayerPrefs.Save();

        Debug.Log("Selected mug index: " +
            selectedMugIndex);

        SceneManager.LoadScene(nextSceneName);
    }
}