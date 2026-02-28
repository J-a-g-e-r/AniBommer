using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI hpText;


    private float maxHP;

    public void Init(float maxHealth)
    {
        maxHP = maxHealth;

        healthSlider.minValue = 0;
        healthSlider.maxValue = maxHP;
        healthSlider.value = maxHP;

        UpdateText(maxHP);
    }

    public void UpdateHealth(float currentHealth)
    {
        healthSlider.value = currentHealth;
        UpdateText(currentHealth);
    }

    private void UpdateText(float currentHealth)
    {
        if (hpText != null)
            hpText.text = currentHealth.ToString();
    }
}