using UnityEngine;

public class ListBombUIShop : MonoBehaviour
{
    [Header("Data")]
    public GameDatabase gameDatabase;

    [Header("UI")]
    public Transform contentParent;
    public BombInforUI itemPrefab;

    private void OnEnable()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (gameDatabase == null || gameDatabase.bombs == null || ShopManager.Instance == null)
            return;

        foreach (BombConfig config in gameDatabase.bombs)
        {
            if (config == null) continue;

            bool isOwned = ShopManager.Instance.IsBombOwned(config.id);
            BombInforUI item = Instantiate(itemPrefab, contentParent);
            item.SetupShop(config, isOwned, OnBuyBomb);
        }
    }

    private void OnBuyBomb(string bombId)
    {
        bool ok = ShopManager.Instance.TryBuyBomb(bombId);
        if (!ok)
            Debug.Log("Buy bomb failed (not enough gold or invalid data)");

        RefreshList();
        ShopManager.Instance.RefreshMoneyUI();
    }
}