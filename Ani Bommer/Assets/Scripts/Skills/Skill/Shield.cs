using System.Collections;
using UnityEngine;

public class Shield : Skill
{
    // Thời gian hồi chiêu
    protected override float Cooldown => 5f;

    // Thời gian tồn tại của khiên
    private const float duration = 5f;

    public Shield()
    {
        // Icon hiển thị trên UI
        icon = Resources.Load<Sprite>("Skills/Icons/Shield");
    }

    protected override void Use(GameObject owner)
    {
        var mono = owner.GetComponent<MonoBehaviour>();
        var userEffects = owner.GetComponent<PlayerEffects>();
        if (mono != null)
        {
            mono.StartCoroutine(Protect(owner));
            userEffects?.PlayShieldSound();
        }
    }

    private IEnumerator Protect(GameObject owner)
    {
        // Load prefab khiên trong Resources
        // Đảm bảo bạn có file: Assets/Resources/Prefabs/MagicShieldBlue.prefab
        var shieldPrefab = Resources.Load<GameObject>("Skills/Prefabs/MagicShieldBlue");
        var stats = owner.GetComponent<PlayerStats>();
        if(stats != null)
        {
            stats.IsInvincible = true; 
        }
        if (shieldPrefab == null)
        {
            Debug.LogError("Shield prefab not found at Resources/Prefabs/MagicShieldBlue");
            yield break;
        }

        // Tạo khiên làm con của player, nên nó sẽ đi theo player
        GameObject shieldInstance = Object.Instantiate(shieldPrefab, owner.transform);
        shieldInstance.transform.localPosition = Vector3.zero; // hoặc offset nhẹ nếu cần

        // Nếu cần, có thể truyền thêm thông tin cho script trên khiên tại đây
        // var behaviour = shieldInstance.GetComponent<ShieldBehaviour>();
        // behaviour.owner = owner;

        // Tồn tại trong duration giây
        yield return new WaitForSeconds(duration);

        // Hủy khiên
        if (shieldInstance != null)
        {
            Object.Destroy(shieldInstance);
        }
        if (stats != null)
        {
            stats.IsInvincible = false;
        }
    }
}
