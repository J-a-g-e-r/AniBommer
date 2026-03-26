using System.Collections;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private SkillListUI skillListUI;


    [Header("Panels")]
    [SerializeField] private RectTransform winPanel;
    [SerializeField] private RectTransform losePanel;
    [SerializeField] private RectTransform pausePanel;

    [Header("Animation Settings")]
    [SerializeField] private float startPosY = 1000f;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float targetY = 0f;
    [SerializeField] private Ease animationEase = Ease.OutBack;


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
        AudioManager.Instance?.PlayWinSound();
        AudioManager.Instance.FadeOutBGM(1.5f);
        await UniTask.Delay(1500); // Đợi 0.5s để đảm bảo mọi thứ đã ổn định trước khi hiển thị panel
        ShowPanel(winPanel);
        losePanel?.gameObject.SetActive(false); // Ẩn panel thua nếu đang hiển thị
        await UniTask.Delay((int)(animationDuration * 1000)); // Đợi animation hoàn thành trước khi phát âm thanh
        Time.timeScale = 0f; // Tạm dừng game khi thắng
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
}
