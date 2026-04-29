using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

[Header("Timer")]
public RectTransform thermometerBar;
public RectTransform thermometerCircle;
public float levelTime = 240f;
// public float thermometerDisplayWidth = 160; // set this in Inspector to match your bar's visual width
private float _thermoWidth;
private float _circleRadius;
private UnityEngine.UI.Image _circleImage;


[Header("Lives")]
public List<Image> lifeImages;        // 3 Image components (Life1, Life2, Life3)
public Sprite mugNormal;
public Sprite mugBroken;

    [Header("Bean Count")]
    public TextMeshProUGUI beanCountText;

    [Header("Chocolate Count")]
    public TextMeshProUGUI chocolateCountText;



    [Header("Level Complete")]
    public GameObject levelCompletePanel;

    [Header("Game Over")]
    public GameObject gameOverPanel;

void Awake()
{
    Debug.Log("UIManager Awake FIRED on " + gameObject.name); // add this first
    if (Instance == null)
    {
        Instance = this;
        Debug.Log("UIManager assigned");
    }
    else
    {
        Debug.Log("UIManager DUPLICATE on " + gameObject.name);
        Destroy(gameObject);
    }
}

void Start()
{
    Debug.Log("UIManager Start FIRED");
    StartCoroutine(InitAfterLayout());
}

IEnumerator InitAfterLayout()
{
    yield return null;
    yield return null;
    _circleImage = thermometerCircle.GetComponent<UnityEngine.UI.Image>();
    _circleRadius = thermometerCircle.rect.width * 0.5f;
    _thermoWidth = thermometerBar.rect.width;
    float halfWidth = _thermoWidth * 0.5f;
    Debug.Log("thermoWidth: " + _thermoWidth + " halfWidth: " + halfWidth);
    thermometerCircle.anchoredPosition = new Vector2(halfWidth - _circleRadius, 0f);
}
public void UpdateTimer(float timeRemaining, bool isTutorial = false)
{
    if (thermometerCircle == null) return;
    float totalTime = LevelManager.Instance != null ? LevelManager.Instance.levelTime : levelTime;
    float t = Mathf.Clamp01(timeRemaining / totalTime);
    float halfWidth = _thermoWidth * 0.5f;
    // t=1 (full time) = right edge, t=0 (no time) = left edge
float x = Mathf.Lerp(halfWidth - _circleRadius, -halfWidth + _circleRadius, t);    thermometerCircle.anchoredPosition = new Vector2(x, 0f);
    if (_circleImage != null)
        _circleImage.color = (timeRemaining <= 30f) ? Color.red : Color.white;
}
    public void UpdateBeans(int collected, int total)
    {
        if (beanCountText == null)
        {
            Debug.LogWarning("UIManager.UpdateBeans: beanCountText is not assigned.");
            return;
        }
        beanCountText.text = collected + "/" + total + " Beans";
    }

    public void UpdateMilk(int collected, int total)
    {
        if (beanCountText == null)
        {
            Debug.LogWarning("UIManager.UpdateMilk: beanCountText is not assigned.");
            return;
        }
        beanCountText.text = collected + "/" + total + " Milk Drops";
    }

    public void UpdateFoam(int collected, int total)
    {
        if (beanCountText == null)
        {
            Debug.LogWarning("UIManager.UpdateFoam: beanCountText is not assigned.");
            return;
        }
        beanCountText.text = collected + "/" + total + " Foams";
    }

    public void UpdateChocolate(int collected, int total)
    {
        if (chocolateCountText != null)
        {
            chocolateCountText.text = collected + "/" + total + " Chocolate Particles";
        }
        else if (beanCountText != null)
        {
            beanCountText.text = collected + "/" + total + " Chocolate Particles";
        }
        else
        {
            Debug.LogWarning("UIManager.UpdateChocolate: no text assigned.");
        }
    }

public void UpdateLives(int lives)
{
    Debug.Log("UpdateLives called with lives=" + lives + ", lifeImages.Count=" + lifeImages.Count 
        + ", mugNormal=" + (mugNormal != null) + ", mugBroken=" + (mugBroken != null));
    
    for (int i = 0; i < lifeImages.Count; i++)
    {
        if (lifeImages[i] == null) continue;
        lifeImages[i].sprite = (i < lives) ? mugNormal : mugBroken;
        lifeImages[i].enabled = true;
    }
}
    public void ShowLevelComplete()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

  public IEnumerator FlashTimer()
{
    for (int i = 0; i < 3; i++)
    {
        if (_circleImage != null)
            _circleImage.color = Color.cyan;
        yield return new WaitForSeconds(0.15f);
        if (_circleImage != null)
            _circleImage.color = Color.white;
        yield return new WaitForSeconds(0.15f);
    }
}
}