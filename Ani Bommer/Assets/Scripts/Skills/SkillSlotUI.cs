using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private Image cooldownOverlay;

    private Skill boundSkill;

    public void Bind(ISkill skill)
    {
        boundSkill = skill as Skill;

        if (boundSkill == null)
        {
            iconImage.enabled = false;
            if (cooldownText != null)
                cooldownText.enabled = false;
            if (cooldownOverlay != null)
                cooldownOverlay.enabled = false;
            return;
        }

        if (boundSkill.icon != null)
        {
            iconImage.sprite = boundSkill.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    private void Update()
    {
        if (boundSkill == null)
            return;

        float cooldownRemaining = boundSkill.CooldownRemaining;

        if (cooldownRemaining > 0f)
        {
            // Hiển thị thời gian hồi chiêu
            if (cooldownText != null)
            {
                cooldownText.text = Mathf.Ceil(cooldownRemaining).ToString();
                cooldownText.enabled = true;
            }

            // Hiển thị overlay cooldown (nếu có)
            if (cooldownOverlay != null)
            {
                float cooldownPercent = cooldownRemaining / boundSkill.CooldownTime;
                cooldownOverlay.fillAmount = cooldownPercent;
                cooldownOverlay.enabled = true;
            }
        }
        else
        {
            // Ẩn text và overlay khi skill sẵn sàng
            if (cooldownText != null)
                cooldownText.enabled = false;
            if (cooldownOverlay != null)
                cooldownOverlay.enabled = false;
        }
    }
}
