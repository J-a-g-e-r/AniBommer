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

            default:
                return null;
        }
    }
}
