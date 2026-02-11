using UnityEngine;

public class SkillListUI : MonoBehaviour
{
    [SerializeField] private SkillSlotUI[] slots;

    public void Bind(PlayerSkills playerSkills)
    {
        var skills = playerSkills.GetSkills();

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Bind(skills[i]);
        }
    }
}
