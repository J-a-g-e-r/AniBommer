using UnityEngine;
using DG.Tweening; // Thư viện DOTween
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private RectTransform preparePanel;
    [SerializeField] private RectTransform taskPanel;
    [SerializeField] private RectTransform shopPanel;
    [SerializeField] private RectTransform adventurePanel;
    [SerializeField] private RectTransform pvpPanel;

    [Header("Settings")]
    [SerializeField] private float duration = 0.5f; // Thời gian trượt
    [SerializeField] private Ease easeType = Ease.OutBack; // Kiểu hiệu ứng trượt


    [Header("PlayerInformation")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI crownsText; // vương miện / kim cương
    [SerializeField] private TextMeshProUGUI goldText;


    [SerializeField] private Animator slideBackground;

    private void Awake()
    {
        // Khởi tạo trạng thái ẩn cho tất cả panel
        InitPanel(preparePanel);
        InitPanel(taskPanel);
        InitPanel(shopPanel);
        InitPanel(adventurePanel);
        InitPanel(pvpPanel);
    }

    private void Start()
    {
        if (DataManager.Instance == null || DataManager.Instance.PlayerData == null)
        {
            Debug.LogWarning("No player data loaded in Lobby.");
            return;
        }

        var data = DataManager.Instance.PlayerData;

        if (playerNameText != null)
            playerNameText.text = data.playerName;

        if (crownsText != null)
            crownsText.text = data.crowns.ToString();

        if (goldText != null)
            goldText.text = data.gold.ToString();
    }

    private void InitPanel(RectTransform panel)
    {
        panel.gameObject.SetActive(false);
        panel.anchoredPosition = new Vector2(1350f, -60f); // Đưa ra ngoài màn hình bên phải
    }

    public void CloseAllPanels()
    {
        // Sử dụng DOTween để trượt panel hiện tại ra ngoài trước khi tắt
        // Ở đây ta tắt nhanh để chuyển sang panel mới
        preparePanel.gameObject.SetActive(false);
        taskPanel.gameObject.SetActive(false);
        shopPanel.gameObject.SetActive(false);
        adventurePanel.gameObject.SetActive(false);
        pvpPanel.gameObject.SetActive(false);
    }

    private void OpenPanelEffect(RectTransform panel)
    {
        CloseAllPanels();
        IsOpenPanel();

        panel.gameObject.SetActive(true);
        // Đặt lại vị trí panel ở bên phải trước khi chạy tween
        panel.anchoredPosition = new Vector2(1350f, -60f);

        // Thực hiện hiệu ứng trượt về vị trí (0,0)
        panel.DOAnchorPos(new Vector2(-100f, -60f), duration).SetEase(easeType);
    }

    public void OpenPrepare() => OpenPanelEffect(preparePanel);
    public void OpenTask() => OpenPanelEffect(taskPanel);
    public void OpenShop() => SceneManager.LoadScene("Shop"); // Mở scene shop thay vì panel
    public void OpenAdventure() => OpenPanelEffect(adventurePanel);
    public void OpenPVP() => SceneManager.LoadScene("PVPScene"); // Mở scene PVP thay vì panel

    private void IsOpenPanel()
    {
        if (slideBackground != null) slideBackground.SetTrigger("IsOpen");
    }

    private void IsClosePanel()
    {
        if (slideBackground != null)
        {
            slideBackground.ResetTrigger("IsOpen");
            slideBackground.SetTrigger("IsClose");
        }
    }

    public void Back()
    {
        IsClosePanel();
        // Hiệu ứng trượt panel hiện tại ra lại bên phải trước khi đóng hoàn toàn
        // (Tùy chọn: tìm panel đang Active và dùng DOAnchorPos ra startPosX)
        CloseAllPanels();
    }
}