using System.Collections;
using UnityEngine;

public class BoostSpeed : Skill
{
    [Header("Skill Info")]
    public string SkillName = "Boost Speed";
    public Sprite Icon => Resources.Load<Sprite>("Icons/BoostSpeed");
    protected override float Cooldown => 5f;

    protected override void Use(GameObject owner)
    {
        owner.GetComponent<MonoBehaviour>().StartCoroutine(Boost(owner));
    }

    private IEnumerator Boost(GameObject owner)
    {
        var stats = owner.GetComponent<PlayerStats>();
        stats.MoveSpeed += 10f;
        yield return new WaitForSeconds(5f);
        stats.MoveSpeed -= 10f;

    }
}
