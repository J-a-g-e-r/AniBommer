using System.Collections;
using UnityEngine;

public class Shield : Skill
{
    protected override float Cooldown => 5f;

    public Shield()
    {
        // TODO: Đảm bảo có file Assets/Resources/Icons/Shield.png
        skillName = "Shield";
        icon = Resources.Load<Sprite>("Icons/Shield");
    }

    protected override void Use(GameObject owner)
    {
        var mono = owner.GetComponent<MonoBehaviour>();
        if (mono != null)
        {
            mono.StartCoroutine(Protect(owner));
        }
    }

    // Unfinished
    private IEnumerator Protect(GameObject owner)
    {
        var stats = owner.GetComponent<PlayerStats>();
        yield return new WaitForSeconds(5f);
    }
}
