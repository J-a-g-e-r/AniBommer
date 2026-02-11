using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public void Bind(ISkill skill)
    {
        if (skill == null)
        {
            iconImage.enabled = false;
            return;
        }

        var baseSkill = skill as Skill;
        if (baseSkill != null && baseSkill.icon != null)
        {
            iconImage.sprite = baseSkill.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }
}
