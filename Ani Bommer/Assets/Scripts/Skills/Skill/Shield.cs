using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : Skill
{
    [Header("Skill Info")]
    public string SkillName = "Boost Speed";
    public Sprite Icon => Resources.Load<Sprite>("Icons/Shield");
    protected override float Cooldown => 5f;

    protected override void Use(GameObject owner)
    {
        owner.GetComponent<MonoBehaviour>().StartCoroutine(Protect(owner));
    }

    // Unfinished
    private IEnumerator Protect(GameObject owner)
    {
        var stats = owner.GetComponent<PlayerStats>();
        yield return new WaitForSeconds(5f);
    }
}
