using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;

    [Header("Money Text")]
    [SerializeField] private TextMeshProUGUI moneyText;

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
        moneyText.text = currentAmount.ToString();
    }

}
