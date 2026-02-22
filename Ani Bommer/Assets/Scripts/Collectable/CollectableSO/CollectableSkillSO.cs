using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Collectable/Skill", fileName = "New Skill Collectable")]
public class CollectableSkillSO : CollectableSOBase
{
    [Header("Skill Settings")]
    public SkillDictionary skillType = SkillDictionary.SpeedBoost;

    public override void Collect(GameObject objectThatCollected)
    {
        var playerSkills = FinderHelper.GetComponentOnObject<PlayerSkills>(objectThatCollected);
        if (playerSkills != null)
        {
            bool success = playerSkills.AddSkill(skillType);
            if (!success)
            {
                // Nếu không còn slot trống, có thể thông báo cho người chơi
                Debug.Log("Không còn slot trống để thêm skill!");
            }
        }

        if (_playerEffects == null)
        {
            GetReference(objectThatCollected);
        }
        _playerEffects.PlayCollectionEffect(CollectClip);
    }
}
