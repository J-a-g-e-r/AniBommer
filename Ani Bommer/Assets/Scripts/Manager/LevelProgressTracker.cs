using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelProgressTracker : MonoBehaviour
{
    [Header("Level Flow")]
    [SerializeField] private string currentLevelScene;
    [SerializeField] private string nextLevelScene;

    [Header("Player Reference")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Star Rules By HP Percent")]
    [SerializeField, Range(0f, 1f)] private float threeStarHpPercent = 0.9f; // >= 90% => 3 sao
    [SerializeField, Range(0f, 1f)] private float oneStarHpPercent = 0.3f;   // < 30% => 1 sao

    private bool levelCompleted;

    public int LastEarnedStars { get; private set; } = 0;
    public int HandleLevelWin()
    {
        if (levelCompleted) return LastEarnedStars;
        levelCompleted = true;
        string levelId = string.IsNullOrWhiteSpace(currentLevelScene)
            ? SceneManager.GetActiveScene().name
            : currentLevelScene;
        int stars = CalculateStars();
        LastEarnedStars = stars;
        if (DataManager.Instance != null)
        {
            DataManager.Instance.CompleteLevel(levelId, stars, nextLevelScene);
        }
        return stars;
    }

    private int CalculateStars()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        if (playerStats == null || playerStats.PlayerHealth <= 0f)
        {
            return 1;
        }

        float hpPercent = playerStats.GetCurrentHealth() / playerStats.PlayerHealth;
        if (hpPercent >= threeStarHpPercent) return 3;
        if (hpPercent < oneStarHpPercent) return 1;
        return 2;
    }
}
