using UnityEngine;

public enum SkillId
{
    Cooking = 0,
    Farming = 1,
    Electricity = 2,
    Crafting = 3,
    Boating = 4,
    Medicine = 5,
    Science = 6,
    Combat = 7,
}

[CreateAssetMenu(fileName = "SkillDefinition", menuName = "Scriptable Objects/SkillDefinition")]
public class SkillDefinition : ScriptableObject
{
    [SerializeField] private SkillId skillId;
    public SkillId SkillId => skillId;

    [SerializeField] private float bonusPerLevel = 0.1f;
    public float BonusPerLevel => bonusPerLevel;

    [SerializeField] private float xpGainRate = 0.1f;
    public float XpGainRate => xpGainRate;

    [SerializeField] private LocalizationItem skillNameLocalization;
    public LocalizationItem SkillNameLocalization => skillNameLocalization;

    [SerializeField] private LocalizationItem skillDescriptionLocalization;
    public LocalizationItem SkillDescriptionLocalization => skillDescriptionLocalization;

    public const int maxSkillLevel = 10;
}