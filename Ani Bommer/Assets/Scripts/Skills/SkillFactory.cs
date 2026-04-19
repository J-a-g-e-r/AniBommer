public static class SkillFactory
{
    public static ISkill CreateSkill(SkillDictionary skill)
    {
        switch (skill)
        {
            case SkillDictionary.SpeedBoost:
                return new BoostSpeed();

            case SkillDictionary.Shield:
                return new Shield();

            case SkillDictionary.Healing:
                return new Healing();
            case SkillDictionary.Slash:
                return new Slash();
            case SkillDictionary.ThrowDagger:
                return new ThrowDagger();
            default:
                return null;
        }
    }
}
