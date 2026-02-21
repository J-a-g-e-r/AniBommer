using UnityEngine;

public abstract class Skill : ISkill
{
    protected abstract float Cooldown { get; }
    public Sprite icon;
    protected float cooldownRemaining;

    public bool CanUse => cooldownRemaining <= 0f;
    public float CooldownRemaining => cooldownRemaining;
    public float CooldownTime => Cooldown;

    public void UpdateCooldown(float deltaTime)
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= deltaTime;
        }
    }

    public void Activate(GameObject owner)
    {
        if (!CanUse) return;

        Use(owner);
        cooldownRemaining = Cooldown;
    }

    protected abstract void Use(GameObject owner);
}