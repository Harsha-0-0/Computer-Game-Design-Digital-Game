using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Settings")]
    public float levelTime = 240f;
    public int totalBeans = 20;
    public bool isTutorial = false;

    private float timeRemaining;
    private int collectedBeans = 0;

    [Header("Milk Settings")]
    public int totalMilk = 40;
    private int collectedMilk = 0;
    public bool useMilkSystem = false;
    private bool levelActive = true;
    private bool levelComplete = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Restore timer from GameManager
        // if it has a saved value
        if (!isTutorial && 
            GameManager.Instance != null)
        {
            float saved = 
                GameManager.Instance.GetSavedTimer();
            // Use saved timer if valid
            timeRemaining = saved > 0 ? 
                saved : levelTime;
        }
        else
        {
            timeRemaining = levelTime;
        }

        collectedBeans = 0;
        collectedMilk = 0;
        levelActive = true;
        levelComplete = false;
        UpdateUI();
    }

    void Update()
    {
        if (!levelActive) return;

        if (isTutorial)
        {
            UpdateUI();
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            UpdateUI();
            TimerRanOut();
        }
        else
        {
            UpdateUI();
        }
    }

    public void BeanCollected()
    {
        collectedBeans++;
        Debug.Log("Beans: " + collectedBeans +
            "/" + totalBeans);
        UpdateUI();

        if (!isTutorial &&
            collectedBeans >= totalBeans)
            LevelComplete();
    }

    public void MugDied()
    {
        if (!levelActive) return;

        if (isTutorial)
        {
            StartCoroutine(RestartLevel());
            return;
        }

        levelActive = false;

        // Save current timer before reload
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveTimer(
                timeRemaining
            );
            GameManager.Instance.LoseLife();

            if (GameManager.Instance.GetLives() > 0)
                StartCoroutine(RestartLevel());
        }
        else
        {
            StartCoroutine(RestartLevel());
        }
    }

    void TimerRanOut()
    {
        if (!levelActive) return;
        levelActive = false;

        Debug.Log("Time ran out!");

        if (GameManager.Instance != null)
            GameManager.Instance.ResetGame();
        else
            SceneManager.LoadScene("Level1");
    }

    void LevelComplete()
    {
        levelComplete = true;
        levelActive = false;
        Debug.Log("Level Complete!");

        // Reset timer for next level
        if (GameManager.Instance != null)
            GameManager.Instance.ResetTimer();

        if (UIManager.Instance != null)
            UIManager.Instance.ShowLevelComplete();
    }

    public void ReduceTime(float seconds)
    {
        if (isTutorial) return;

        timeRemaining -= seconds;
        if (timeRemaining < 0)
            timeRemaining = 0;

        // Save updated timer
        if (GameManager.Instance != null)
            GameManager.Instance.SaveTimer(
                timeRemaining
            );

        if (UIManager.Instance != null)
            StartCoroutine(
                UIManager.Instance.FlashTimer()
            );

        UpdateUI();
    }

    IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    void UpdateUI()
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.UpdateTimer(
            timeRemaining, isTutorial
        );
        if (useMilkSystem)
        {
            UIManager.Instance.UpdateMilk(collectedMilk, totalMilk);
        }
        else
        {
            UIManager.Instance.UpdateBeans(collectedBeans, totalBeans);
        }

        int lives = GameManager.Instance != null ?
            GameManager.Instance.GetLives() : 5;
        UIManager.Instance.UpdateLives(lives);
    }

    public int GetBeans() { return collectedBeans; }
    public float GetTime() { return timeRemaining; }
    public void MilkCollected(int amount = 1)
    {
        collectedMilk += amount;
        collectedMilk = Mathf.Max(collectedMilk, 0);

        Debug.Log("Milk: " + collectedMilk + "/" + totalMilk);
        UpdateUI();
    }

    public void LoseMilk(int amount = 1)
    {
        collectedMilk -= amount;
        collectedMilk = Mathf.Max(collectedMilk, 0);

        Debug.Log("Milk lost: " + collectedMilk + "/" + totalMilk);
        UpdateUI();
    }

    public int GetMilk()
    {
        return collectedMilk;
    }

    public int GetTotalMilk()
    {
        return totalMilk;
    }
}