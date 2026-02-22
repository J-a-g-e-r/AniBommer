using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkills : MonoBehaviour
{
    private ISkill[] skills = new ISkill[3];

    public void Init(Characters character)
    {
        skills[0] = SkillFactory.CreateSkill(character.startingSkill);
        skills[1] = null;
        skills[2] = null;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < skills.Length; i++) 
        {
            skills[i]?.UpdateCooldown(dt);
        }
    }

    public void UseSkill(int index)
    {
        skills[index]?.Activate(gameObject);
    }

    public ISkill[] GetSkills() => skills;

    /// <summary>
    /// Thêm skill vào slot trống đầu tiên. Trả về true nếu thêm thành công, false nếu không còn slot trống.
    /// </summary>
    public bool AddSkill(SkillDictionary skillType)
    {
        if (skillType == SkillDictionary.None)
            return false;

        // Tìm slot trống đầu tiên
        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i] == null)
            {
                skills[i] = SkillFactory.CreateSkill(skillType);
                GameEvents.OnSkillAdded?.Invoke();
                return true;
            }
        }

        return false; // Không còn slot trống
    }
}
