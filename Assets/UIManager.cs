using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Timer")]
    public TextMeshProUGUI timerText;

    [Header("Bean Count")]
    public TextMeshProUGUI beanCountText;

    [Header("Lives")]
    public List<GameObject> lifeIcons;

    [Header("Level Complete")]
    public GameObject levelCompletePanel;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void UpdateTimer(
        float timeRemaining, 
        bool isTutorial = false)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(
            timeRemaining / 60f
        );
        int seconds = Mathf.FloorToInt(
            timeRemaining % 60f
        );

        if (isTutorial)
        {
            timerText.text = "Tutorial";
            timerText.color = Color.white;
        }
        else
        {
            timerText.text = string.Format(
                "{0:00}:{1:00}", minutes, seconds
            );

            // Turn red when less than 30 seconds
            if (timeRemaining <= 30f)
                timerText.color = Color.red;
            else
                timerText.color = Color.white;
        }
    }

    public void UpdateBeans(int collected, int total)
    {
        if (beanCountText == null) return;
        beanCountText.text = collected + 
            "/" + total + " Beans";
    }

    public void UpdateMilk(int collected, int total)
    {
        if (beanCountText == null) return;
        beanCountText.text = collected + "/" + total + " Milk Drops";
    }

    public void UpdateLives(int lives)
    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] != null)
                lifeIcons[i].SetActive(i < lives);
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
            if (timerText != null)
                timerText.color = Color.cyan;
            yield return new WaitForSeconds(0.15f);
            if (timerText != null)
                timerText.color = Color.white;
            yield return new WaitForSeconds(0.15f);
        }
    }
}