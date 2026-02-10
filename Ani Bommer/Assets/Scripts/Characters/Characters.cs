using UnityEngine;

[CreateAssetMenu(fileName = "Characters", menuName = "Ani Bommer/Characters")]
public class Characters : ScriptableObject
{
    [Header("Stats")]
    public float moveSpeed;
    public int bombRange;
    public int maxBombs;


    [Header("Starting SKill{")]
    public SkillDictionary startingSkill;
}
