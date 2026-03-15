using UnityEngine;
using TMPro; // Nếu bạn dùng TextMeshPro

public class LevelSelection : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mapSelectionView;
    public GameObject levelSelectionView;

    [Header("Level Lists")]
    public GameObject christmasLevelList;
    public GameObject pirateLevelList;

    [Header("UI Elements")]
    public TextMeshProUGUI mapTitleText;

    // Hàm gọi khi chọn Map Giáng Sinh
    public void SelectChristmasMap()
    {
        ShowLevelView("Giáng Sinh", christmasLevelList);
    }

    // Hàm gọi khi chọn Map Hải Tặc
    public void SelectPirateMap()
    {
        ShowLevelView("Hải Tặc", pirateLevelList);
    }

    private void ShowLevelView(string title, GameObject levelListToShow)
    {
        // Ẩn bảng chọn map, hiện bảng chọn level
        mapSelectionView.SetActive(false);
        levelSelectionView.SetActive(true);

        // Tắt tất cả các danh sách trước khi bật cái cần thiết
        christmasLevelList.SetActive(false);
        pirateLevelList.SetActive(false);

        // Bật danh sách tương ứng và đổi tên tiêu đề
        levelListToShow.SetActive(true);
        mapTitleText.text = title;
    }

    public void BackToMapSelection()
    {
        levelSelectionView.SetActive(false);
        mapSelectionView.SetActive(true);
    }
}