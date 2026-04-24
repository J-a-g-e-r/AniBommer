using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthBar;

    private void Start()
    {
        healthBar.gameObject.SetActive(false);
    }

    public void GetBossHP(GameObject monster)
    {
        int hp = monster.GetComponent<Monster>().GetMaxHP();
        healthBar.maxValue = hp;
        healthBar.value = hp;                 // thêm
        healthBar.gameObject.SetActive(true); // thêm
    }

    public void UpdateBossHP(int currentHP)
    {
        healthBar.value = currentHP;
    }
}
