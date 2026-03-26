using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager instance;

    public int CurrentMoney { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Đồng bộ tiền từ save
        if (DataManager.Instance != null && DataManager.Instance.PlayerData != null)
        {
            CurrentMoney = DataManager.Instance.PlayerData.gold;
            HUDManager.instance?.UpdateMoneyText(CurrentMoney);
        }
    }

    public void IncreaseMoney(int amount)
    {
        if (amount <= 0) return;
        CurrentMoney += amount;
        DataManager.Instance.PlayerData.gold = CurrentMoney;
        DataManager.Instance.SavePlayerData();
        HUDManager.instance?.UpdateMoneyText(CurrentMoney);
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount <= 0) return true;
        if (CurrentMoney < amount) return false;

        CurrentMoney -= amount;
        DataManager.Instance.PlayerData.gold = CurrentMoney;
        DataManager.Instance.SavePlayerData();
        HUDManager.instance?.UpdateMoneyText(CurrentMoney);
        return true;
    }
}
