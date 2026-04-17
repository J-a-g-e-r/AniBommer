using System.Collections;
using UnityEngine;

public class BoostSpeed : Skill
{

    protected override float Cooldown => 10f;
    public BoostSpeed()
    {
        // Gán thông tin hiển thị cho skill (dùng trong UI)
        icon = Resources.Load<Sprite>("Skills/Icons/BoostSpeed");
    }

    protected override void Use(GameObject owner)
    {
        var mono = owner.GetComponent<MonoBehaviour>();
        var userEffects = owner.GetComponent<PlayerEffects>();
        if (mono != null)
        {
            mono.StartCoroutine(Boost(owner));
            userEffects?.PlayBoostSpeedSound();
        }
    }

    private IEnumerator Boost(GameObject owner)
    {
        // Thử lấy stats network trước, nếu không có thì dùng offline
        var statsNetwork = owner.GetComponent<PlayerStatsNetwork>();
        var stats = owner.GetComponent<PlayerStats>();

        if (statsNetwork != null)
        {
            statsNetwork.MoveSpeed += 10f;
            yield return new WaitForSeconds(5f);
            statsNetwork.MoveSpeed -= 10f;
        }
        else if (stats != null)
        {
            stats.MoveSpeed += 10f;
            yield return new WaitForSeconds(5f);
            stats.MoveSpeed -= 10f;
        }
    }
}
