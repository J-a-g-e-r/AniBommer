using System.Collections;
using UnityEngine;

public class BoostSpeed : Skill
{
    protected override float Cooldown => 10f;

    public BoostSpeed()
    {
        // Gán thông tin hiển thị cho skill (dùng trong UI)
        skillName = "Boost Speed";
        icon = Resources.Load<Sprite>("Icons/BoostSpeed");
    }

    protected override void Use(GameObject owner)
    {
        var mono = owner.GetComponent<MonoBehaviour>();
        if (mono != null)
        {
            mono.StartCoroutine(Boost(owner));
        }
    }

    private IEnumerator Boost(GameObject owner)
    {
        var stats = owner.GetComponent<PlayerStats>();
        stats.MoveSpeed += 10f;
        yield return new WaitForSeconds(5f);
        stats.MoveSpeed -= 10f;
    }
}
