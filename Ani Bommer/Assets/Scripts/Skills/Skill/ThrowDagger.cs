using UnityEngine;

public class ThrowDagger : Skill
{
    protected override float Cooldown => 3f;

    // Đặt prefab tại: Assets/Resources/Skills/Prefabs/ThrownDagger.prefab
    private const string PrefabResourcesPath = "Skills/Prefabs/Dagger";

    public ThrowDagger()
    {
        icon = Resources.Load<Sprite>("Skills/Icons/Dagger");
    }

    protected override void Use(GameObject owner)
    {
        GameObject prefab = Resources.Load<GameObject>(PrefabResourcesPath);
        if (prefab == null)
        {
            Debug.LogError("Không tìm thấy prefab tại Resources/" + PrefabResourcesPath);
            return;
        }

        Vector3 origin = owner.transform.position + Vector3.up ;
        Vector3 dir = owner.transform.forward;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
            dir = Vector3.forward;
        dir.Normalize();

        Quaternion rot = Quaternion.LookRotation(dir);
        var userEffects = owner.GetComponent<PlayerEffects>();
        userEffects?.PlayThrowClip();
        Object.Instantiate(prefab, origin, rot);
    }
}