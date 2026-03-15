using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Nếu bạn có sử dụng DOTween để di chuyển mượt hơn

public class MapSelection : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform contentPanel;
    public Button btnLeft, btnRight;

    [SerializeField] int totalMaps = 2;
    private int currentMapIndex = 0;

    void Start()
    {
        //btnLeft.onClick.AddListener(PrevMap);
        //btnRight.onClick.AddListener(NextMap);
        UpdateButtons();
    }

    public void NextMap()
    {
        if (currentMapIndex < totalMaps - 1)
        {
            currentMapIndex++;
            ScrollToMap();
        }
    }

    public void PrevMap()
    {
        if (currentMapIndex > 0)
        {
            currentMapIndex--;
            ScrollToMap();
        }
    }

    void ScrollToMap()
    {
        // Tính toán vị trí ngang (0 đến 1)
        float targetNormalizedPos = (float)currentMapIndex / (totalMaps - 1);

        //// Di chuyển mượt bằng LeanTween hoặc DOTween (nếu có)
        scrollRect.DOHorizontalNormalizedPos(targetNormalizedPos, 0.5f);

        //// Hoặc dùng mặc định của Unity (không mượt bằng)
        //scrollRect.horizontalNormalizedPosition = targetNormalizedPos;

        UpdateButtons();
    }

    void UpdateButtons()
    {
        btnLeft.interactable = currentMapIndex > 0;
        btnRight.interactable = currentMapIndex < totalMaps - 1;
    }
}