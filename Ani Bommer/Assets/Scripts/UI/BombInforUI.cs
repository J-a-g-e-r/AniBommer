using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BombInforUI : MonoBehaviour
{
    [Header("UI")]
    public Image bombImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    [Header("Shop UI")]
    public TMP_Text priceText;

    public Button selectButton;
    public TMP_Text selectButtonText;

    private string _bombId;
    private System.Action<string> _onSelect;
    private System.Action<string> _onBuy;
    public void Setup(BombConfig config, bool isEquipped, System.Action<string> onSelect)
    {
        _bombId = config.id;
        _onSelect = onSelect;

        bombImage.sprite = config.sprite;
        nameText.text = config.displayName;
        descriptionText.text = config.description;

        if (isEquipped)
        {
            selectButton.interactable = false;
            selectButtonText.text = "IN USED";
        }
        else
        {
            selectButton.interactable = true;
            selectButtonText.text = "SELECT";
        }

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnClickSelect);
    }

    private void OnClickSelect()
    {
        _onSelect?.Invoke(_bombId);
    }

    public void SetupShop(BombConfig config, bool isOwned, System.Action<string> onBuy)
    {
        if (config == null) return;

        _bombId = config.id;
        _onBuy = onBuy;
        _onSelect = null;

        bombImage.sprite = config.sprite;
        nameText.text = config.displayName;
        descriptionText.text = config.description;


        if (priceText != null)
        {
            priceText.gameObject.SetActive(true);
            priceText.text = config.priceGold.ToString();
        }

        if (isOwned)
        {
            selectButton.interactable = false;
            selectButtonText.text = "OWNED";
        }
        else
        {
            selectButton.interactable = true;
            selectButtonText.text = $"${config.priceGold}";
        }

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnClickBuy);
    }

    private void OnClickBuy()
    {
        _onBuy?.Invoke(_bombId);
    }
}