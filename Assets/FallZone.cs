using UnityEngine;
using UnityEngine.SceneManagement;

public class FallZone : MonoBehaviour
{
    private bool isTriggered = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Mug") && !isTriggered)
        {
            isTriggered = true;

            string currentScene = SceneManager
                .GetActiveScene().name;

            if (currentScene == "Level_2")
            {
                // Use LevelManager_2 for Level 2
                LevelManager_2 lm2 =
                    FindObjectOfType<LevelManager_2>();

                if (lm2 != null)
                    lm2.OnMugFell();
                else
                    Debug.LogError(
                        "FallZone: LevelManager_2 " +
                        "not found in Level2!");
            }
            else
            {
                // Use LevelManager for all other scenes
                // (Tutorial, Level1, Level3, Level4)
                if (LevelManager.Instance != null)
                    LevelManager.Instance.MugDied();
                else
                    Debug.LogError(
                        "FallZone: LevelManager.Instance " +
                        "is null in " + currentScene);
            }

            // Reset after delay so it can trigger again
            // after mug respawns
            Invoke(nameof(ResetTrigger), 2f);
        }
    }

    void ResetTrigger()
    {
        isTriggered = false;
    }
}