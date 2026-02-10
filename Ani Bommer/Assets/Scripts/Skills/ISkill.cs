using UnityEngine;

public interface ISkill
{
    void Activate(GameObject owner);
    void UpdateCooldown(float deltaTime);
}