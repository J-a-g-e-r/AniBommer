using UnityEngine;
using UnityEngine.UI;

public class PreparePanelManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button characterButton;
    public Button bombButton;

    [Header("Panels")]
    public GameObject listCharacter;
    public GameObject listBomb;

    void OnEnable()
    {
        // Khi mở Prepare Panel, mặc định chọn Nhân Vật
        ShowCharacterList();
    }

    public void ShowCharacterList()
    {
        // Bật list nhân vật, tắt list bom
        listCharacter.SetActive(true);
        listBomb.SetActive(false);

        // Hiệu ứng hình ảnh cho button (tùy chọn)
        SetButtonAlpha(characterButton, 1f);
        SetButtonAlpha(bombButton, 0.5f);
    }

    public void ShowBombList()
    {
        // Bật list bom, tắt list nhân vật
        listCharacter.SetActive(false);
        listBomb.SetActive(true);

        // Hiệu ứng hình ảnh cho button (tùy chọn)
        SetButtonAlpha(characterButton, 0.5f);
        SetButtonAlpha(bombButton, 1f);
    }

    private void SetButtonAlpha(Button btn, float alpha)
    {
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = alpha;
    }
}