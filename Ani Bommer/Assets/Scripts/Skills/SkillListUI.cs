using UnityEngine;

public class SkillListUI : MonoBehaviour
{
    [SerializeField] private SkillSlotUI[] slots;
    private PlayerSkills boundPlayerSkills;

    public void Bind(PlayerSkills playerSkills)
    {
        boundPlayerSkills = playerSkills;
        Refresh();
    }

    public void Refresh()
    {
        if (boundPlayerSkills == null) return;

        var skills = boundPlayerSkills.GetSkills();

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Bind(skills[i]);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnSkillAdded += Refresh;
    }

    private void OnDisable()
    {
        GameEvents.OnSkillAdded -= Refresh;
    }
}
