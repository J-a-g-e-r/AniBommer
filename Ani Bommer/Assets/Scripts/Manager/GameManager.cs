using System.Collections;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private SkillListUI skillListUI;
    [SerializeField] private LevelProgressTracker levelProgressTracker;


    [Header("Panels")]
    [SerializeField] private RectTransform winPanel;
    [SerializeField] private RectTransform losePanel;
    [SerializeField] private RectTransform pausePanel;

    [Header("Animation Settings")]
    [SerializeField] private float startPosY = 1000f;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float targetY = 0f;
    [SerializeField] private Ease animationEase = Ease.OutBack;

    [Header("Win Star Animation")]
    [SerializeField] private RectTransform starFlyStartPoint;
    [SerializeField] private RectTransform[] emptyStarSlots; // 3 slot trống
    [SerializeField] private CanvasGroup[] filledStarGroups; // 3 sao lấp
    [SerializeField] private float starFlyDuration = 0.35f;
    [SerializeField] private float starDelayBetween = 0.1f;
    [SerializeField] private float starPunchScale = 0.25f;
    [SerializeField] private GameObject starEffect;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitPanel(winPanel);
        InitPanel(losePanel);
        InitPanel(pausePanel);
    }

    private void InitPanel(RectTransform panel)
    {
        if (panel == null) return;
        panel.gameObject.SetActive(false);
        var pos = panel.anchoredPosition;
        pos.y = startPosY;
        panel.anchoredPosition = pos;
    }



    public void RegisterLocalPlayer(GameObject player)
    {
        Debug.Log("🔥 RegisterLocalPlayer CALLED");
        StartCoroutine(BindPlayerSkillsNextFrame(player));
        GameEvents.OnPlayerSpawned?.Invoke();
    }

    private IEnumerator BindPlayerSkillsNextFrame(GameObject player)
    {
        // Chờ 1 frame để các Start()/Init() của Player chạy xong
        yield return null;

        var playerSkills = player.GetComponent<PlayerSkills>();

        skillListUI.Bind(playerSkills);
    }


    public async void OnGameWin()
    {
        levelProgressTracker?.HandleLevelWin();
        AudioManager.Instance?.PlayWinSound();
        AudioManager.Instance.FadeOutBGM(1.5f);
        await UniTask.Delay(1500); // Đợi 0.5s để đảm bảo mọi thứ đã ổn định trước khi hiển thị panel
        ShowPanel(winPanel);
        losePanel?.gameObject.SetActive(false); // Ẩn panel thua nếu đang hiển thị

        if (levelProgressTracker != null)
        {
            int starsEarned = levelProgressTracker.LastEarnedStars;
            StartCoroutine(PlayWinStarsAnimation(starsEarned));
            await UniTask.Delay((int)(animationDuration * 1000)); // Đợi animation hoàn thành trước khi phát âm thanh
        }
        else
        {
            await UniTask.Delay((int)(animationDuration * 1000)); // Đợi animation hoàn thành trước khi phát âm thanh
            Time.timeScale = 0f; // Tạm dừng game khi thắng
        }

    }

    public async void OnGameLose()
    {
        AudioManager.Instance?.PlayLoseSound();
        AudioManager.Instance.FadeOutBGM(1.5f);
        await UniTask.Delay(1500); // Đợi 0.5s để đảm bảo mọi thứ đã ổn định trước khi hiển thị panel
        ShowPanel(losePanel);
        winPanel?.gameObject.SetActive(false); // Ẩn panel thắng nếu đang hiển thị
        await UniTask.Delay((int)(animationDuration * 1000)); // Đợi animation hoàn thành trước khi phát âm thanh
        Time.timeScale = 0f; // Tạm dừng game khi thua
    }

    public async void OnGamePause()
    {
        ShowPanel(pausePanel);
        await UniTask.Delay((int)(animationDuration * 1000)); // Đợi animation hoàn thành trước khi phát âm thanh
        Time.timeScale = 0f; // Tạm dừng game khi pause
    }

    private void ShowPanel(RectTransform panel)
    {
        if(panel == null) return;
        panel.gameObject.SetActive(true);

        var start = panel.anchoredPosition;
        start.y = startPosY;
        panel.anchoredPosition = start;

        panel.DOAnchorPosY(targetY, animationDuration).SetEase(animationEase);
    }


    public void RestartGame()
    {
        Time.timeScale = 1f; // Đảm bảo game không bị tạm dừng
        AudioManager.Instance.RestartBGM();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void Home()
    {
        Time.timeScale = 1f; // Đảm bảo game không bị tạm dừng
        UnityEngine.SceneManagement.SceneManager.LoadScene(2); // Giả sử scene 0 là menu chính
    }

    public void Lobby()
    {
        Time.timeScale = 1f;

        // 1) Rời lobby/network trước (nếu có)
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.LeaveLobby();
        }

        // 2) Tắt overlay đen nếu đang còn
        if (LoadingFadeEffect.Instance != null)
        {
            LoadingFadeEffect.Instance.FadeOut();
        }

        // 3) Load LobbyScene
        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.LoadScene("LobbyScene");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        }
    }

    public void Continue()
    {
        Time.timeScale = 1f; // Tiếp tục game
        if (pausePanel != null)
        {
            var start = pausePanel.anchoredPosition;
            start.y = startPosY;
            pausePanel.anchoredPosition = start;
            pausePanel.gameObject.SetActive(false);
        }
    }

    private void ResetWinStarsVisual()
    {
        for (int i = 0; i < filledStarGroups.Length; i++)
        {
            if (filledStarGroups[i] == null) continue;
            filledStarGroups[i].alpha = 0f;
            var rt = filledStarGroups[i].GetComponent<RectTransform>();
            rt.anchoredPosition = starFlyStartPoint.anchoredPosition;
            rt.localScale = Vector3.one * 0.3f;
        }
    }

    private IEnumerator PlayWinStarsAnimation(int stars)
    {
        stars = Mathf.Clamp(stars, 0, 3);
        ResetWinStarsVisual();
        for (int i = 0; i < stars; i++)
        {
            CanvasGroup star = filledStarGroups[i];
            RectTransform targetSlot = emptyStarSlots[i];
            if (star == null || targetSlot == null) continue;
            RectTransform starRT = star.GetComponent<RectTransform>();
            starRT.anchoredPosition = starFlyStartPoint.anchoredPosition;
            starRT.localScale = Vector3.one * 0.3f;
            star.alpha = 1f;
            Sequence seq = DOTween.Sequence();
            seq.Join(starRT.DOAnchorPos(targetSlot.anchoredPosition, starFlyDuration).SetEase(Ease.OutCubic));
            seq.Join(starRT.DOScale(1f, starFlyDuration).SetEase(Ease.OutBack));
            seq.Append(starRT.DOPunchScale(Vector3.one * starPunchScale, 0.2f, 8, 0.8f));
            yield return seq.WaitForCompletion();
            AudioManager.Instance.PlayStarSoundByIndex(i);
            Instantiate(starEffect, targetSlot, false);
            yield return new WaitForSeconds(starDelayBetween);
        }

    }
}
