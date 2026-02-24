using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;

    [Header("Money Text")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI maxBombText;
    [SerializeField] private TextMeshProUGUI bombRangeText;
    [SerializeField] private TextMeshProUGUI speedText;

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

    public void UpdateMoneyText(int currentAmount)
    {
        moneyText.text = "x" + currentAmount.ToString();
    }

    public void UpdateMaxBombText(int currentAmount)
    {
        maxBombText.text ="x" + currentAmount.ToString();
        if(currentAmount == 8)
        {
            maxBombText.color = Color.yellow;
        }
    }

    public void UpdateBombRangeText(int currentAmount)
    {
        bombRangeText.text = "x" + currentAmount.ToString();
        if (currentAmount == 6)
        {
            bombRangeText.color = Color.yellow;
        }
    }

    public void UpdateSpeedText(float currentAmount)
    {
        speedText.text = "x" + (currentAmount-7).ToString();
        if (currentAmount == 15)
        {
            speedText.color = Color.yellow;
        }
    }

    public void InitPlayerStats(float health,int maxBomb, int bombRange, float speed)
    {
        UpdateMaxBombText(maxBomb);
        UpdateBombRangeText(bombRange);
        UpdateSpeedText(speed);
    }
}
