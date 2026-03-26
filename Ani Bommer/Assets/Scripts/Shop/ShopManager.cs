using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Money")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI crownText;

    [Header("Data")]
    [SerializeField] private GameDatabase gameDatabase;


    private PlayerData PlayerData => DataManager.Instance.PlayerData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RefreshMoneyUI();
    }

    public bool IsCharacterOwned(string characterId)
    {
        return PlayerData != null && PlayerData.ownedCharacters.Contains(characterId);
    }

    public bool IsBombOwned(string bombId)
    {
        return PlayerData != null && PlayerData.ownedBombs.Contains(bombId);
    }

    public bool TryBuyCharacter(string characterId)
    {
        if (PlayerData == null || gameDatabase == null || MoneyManager.instance == null) return false;

        CharacterConfig cfg = gameDatabase.GetCharacter(characterId);
        if (cfg == null) return false;

        if (PlayerData.ownedCharacters.Contains(characterId))
            return true; // đã có rồi

        if (!MoneyManager.instance.TrySpendMoney(cfg.priceGold))
            return false; // không đủ tiền

        PlayerData.ownedCharacters.Add(characterId);
        DataManager.Instance.SavePlayerData();
        return true;
    }

    public bool TryBuyBomb(string bombId)
    {
        if (PlayerData == null || gameDatabase == null || MoneyManager.instance == null) return false;

        BombConfig cfg = gameDatabase.GetBomb(bombId);
        if (cfg == null) return false;

        if (PlayerData.ownedBombs.Contains(bombId))
            return true; // đã có rồi

        if (!MoneyManager.instance.TrySpendMoney(cfg.priceGold))
            return false; // không đủ tiền

        PlayerData.ownedBombs.Add(bombId);
        DataManager.Instance.SavePlayerData();
        return true;
    }

    public void RefreshMoneyUI()
    {
        if (goldText != null)
            goldText.text = PlayerData != null ? PlayerData.gold.ToString() : "0";

        if (crownText != null)
            crownText.text = PlayerData != null ? PlayerData.crowns.ToString() : "0";
    }

    public void BackToLobby()
    {
        SceneManager.LoadScene(2);
    }
}