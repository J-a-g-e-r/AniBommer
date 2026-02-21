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

    public void IncreaseMoney(int amount)
    {
        CurrentMoney += amount;
        HUDManager.instance.UpdateMoneyText(CurrentMoney);

    }
}
