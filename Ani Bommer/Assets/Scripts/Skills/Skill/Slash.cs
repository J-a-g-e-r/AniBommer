using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : Skill
{
    // Thời gian hồi chiêu
    protected override float Cooldown => 5f;

    public Slash()
    {
        // Icon hiển thị trên UI
        icon = Resources.Load<Sprite>("Skills/Icons/Slash");
    }

    protected override void Use(GameObject owner)
    {
        var mono = owner.GetComponent<MonoBehaviour>();
        var userEffects = owner.GetComponent<PlayerEffects>();
        if (mono != null)
        {
            mono.StartCoroutine(SlashTrail(owner));
            //userEffects?.PlayShieldSound();
        }
    }

    private IEnumerator SlashTrail(GameObject owner)
    {
        var slashPrefab = Resources.Load<GameObject>("Skills/Prefabs/SwordTrail");
        if (slashPrefab == null)
        {
            Debug.LogError("Healing prefab not found at Resources/Skills/Prefabs/HealingCircle");
            yield break;
        }

        GameObject slashInstance = Object.Instantiate(slashPrefab, owner.transform.position + Vector3.up, owner.transform.rotation *Quaternion.Euler(90f, 0f, 0f));
        // Giữ trong 4s
        yield return new WaitForSeconds(1f);

        if (slashInstance != null)
        {
            Object.Destroy(slashInstance);
        }
    }
}
