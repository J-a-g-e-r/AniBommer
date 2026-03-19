using System.Collections;
using UnityEngine;

public class Healing : Skill
{
    // Thời gian hồi chiêu
    protected override float Cooldown => 5f;

    // Thời gian tồn tại của vùng hồi máu
    private const float duration = 4f;

    public Healing()
    {
        // Icon hiển thị trên UI
        icon = Resources.Load<Sprite>("Skills/Icons/Healing");
    }

    protected override void Use(GameObject owner)
    {
        var mono = owner.GetComponent<MonoBehaviour>();
        var userEffects = owner.GetComponent<PlayerEffects>();
        if (mono != null)
        {
            mono.StartCoroutine(SpawnHealingZone(owner));
            userEffects?.PlayHealingSound();
        }
    }

    private IEnumerator SpawnHealingZone(GameObject owner)
    {
        // Load prefab vùng hồi máu
        // Ví dụ: Assets/Resources/Skills/Prefabs/HealingCircle.prefab
        var healPrefab = Resources.Load<GameObject>("Skills/Prefabs/HealingCircle");
        if (healPrefab == null)
        {
            Debug.LogError("Healing prefab not found at Resources/Skills/Prefabs/HealingCircle");
            yield break;
        }

        // Spawn tại vị trí người chơi (không cần làm con, vùng đứng yên)
        GameObject healInstance = Object.Instantiate(
            healPrefab,
            owner.transform.position,
            Quaternion.identity
        );

        // Giữ trong 4s
        yield return new WaitForSeconds(duration);

        if (healInstance != null)
        {
            Object.Destroy(healInstance);
        }
    }
}