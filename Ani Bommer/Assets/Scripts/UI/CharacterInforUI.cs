using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterInforUI : MonoBehaviour
{
    [Header("UI")]
    public Image avatarImage;
    public TMP_Text nameText;
    public TMP_Text hpText;
    public TMP_Text speedText;
    public TMP_Text rangeText;
    public TMP_Text maxBombText;
    public TMP_Text description;

    [Header("Shop UI")]
    public TMP_Text priceText;

    public Button selectButton;
    public TMP_Text selectButtonText;

    
    private string _characterId;
    private System.Action<string> _onSelect;

    private System.Action<string> _onBuy;


    public void Setup(CharacterConfig config, bool isEquipped, System.Action<string> onSelect)
    {
        _characterId = config.id;
        _onSelect = onSelect;

        avatarImage.sprite = config.sprite;
        nameText.text = config.displayName;
        
        if(config.stats != null)
        {
            hpText.text = config.stats.playerHealth.ToString();
            speedText.text = config.stats.moveSpeed.ToString();
            rangeText.text = config.stats.bombRange.ToString();
            maxBombText.text = config.stats.maxBombs.ToString();
            //description.text = config.description;
        }

        if (isEquipped)
        {
            selectButton.interactable = false;
            selectButtonText.text = "IN USED";
            //selectButton.GetComponent<Image>().color = Color.gray;
        }
        else
        {
            selectButton.interactable = true;
            selectButtonText.text = "SELECT";
            //selectButton.GetComponent<Image>().color = Color.white;
        }

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnClickSelect);
    }

    private void OnClickSelect()
    {
        _onSelect?.Invoke(_characterId);
    }


    public void SetupShop(CharacterConfig config, bool isOwned, System.Action<string> onBuy)
    {
        if (config == null) return;

        _characterId = config.id;
        _onBuy = onBuy;
        _onSelect = null;

        avatarImage.sprite = config.sprite;
        nameText.text = config.displayName;

        if (config.stats != null)
        {
            hpText.text = config.stats.playerHealth.ToString();
            speedText.text = config.stats.moveSpeed.ToString();
            rangeText.text = config.stats.bombRange.ToString();
            maxBombText.text = config.stats.maxBombs.ToString();
            description.text = config.description;
        }

        // Hiện giá
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
        _onBuy?.Invoke(_characterId);
    }
}
