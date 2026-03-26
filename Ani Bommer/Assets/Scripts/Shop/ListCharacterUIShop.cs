using UnityEngine;

public class ListCharacterUIShop : MonoBehaviour
{
    [Header("Data")]
    public GameDatabase gameDatabase;

    [Header("UI")]
    public Transform contentParent;
    public CharacterInforUI itemPrefab;

    private void Start()
    {
        RefreshList();
    }

    private void OnEnable()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (gameDatabase == null || gameDatabase.characters == null || ShopManager.Instance == null)
            return;

        foreach (CharacterConfig config in gameDatabase.characters)
        {
            if (config == null) continue;

            bool isOwned = ShopManager.Instance.IsCharacterOwned(config.id);
            CharacterInforUI item = Instantiate(itemPrefab, contentParent);
            item.SetupShop(config, isOwned, OnBuyCharacter);
        }
    }

    private void OnBuyCharacter(string characterId)
    {
        bool ok = ShopManager.Instance.TryBuyCharacter(characterId);
        if (!ok)
            Debug.Log("Buy character failed (not enough gold or invalid data)");

        RefreshList();
        ShopManager.Instance.RefreshMoneyUI();
    }
}