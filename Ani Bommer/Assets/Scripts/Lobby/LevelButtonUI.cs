using UnityEngine;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
    [Header("Level Config")]
    [SerializeField] private string levelSceneName;

    [Header("UI")]
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite starOnSprite;
    [SerializeField] private Sprite starOffSprite;

    private void OnEnable()
    {
        RefreshView();
    }

    public void RefreshView()
    {
        if (string.IsNullOrWhiteSpace(levelSceneName)) return;

        bool isUnlocked = DataManager.Instance != null && DataManager.Instance.IsLevelUnlocked(levelSceneName);
        int stars = DataManager.Instance != null ? DataManager.Instance.GetLevelStars(levelSceneName) : 0;

        if (playButton != null)
        {
            playButton.interactable = isUnlocked;
            playButton.onClick.RemoveAllListeners(); // tránh bị add nhiều lần
            playButton.onClick.AddListener(OnClickPlayLevel);
        }

        if (lockObject != null)
        {
            lockObject.SetActive(!isUnlocked);
        }

        if (starImages == null) return;
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            if (starOnSprite != null && starOffSprite != null)
            {
                starImages[i].sprite = i < stars ? starOnSprite : starOffSprite;
            }

            Color c = starImages[i].color;
            c.a = i < stars ? 1f : 0.35f;
            starImages[i].color = c;
        }
    }

    public void OnClickPlayLevel()
    {
        if (DataManager.Instance == null || !DataManager.Instance.IsLevelUnlocked(levelSceneName)) return;

        DataManager.Instance.SetCurrentLevel(levelSceneName);
        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.LoadScene(levelSceneName);
        }
    }
}
