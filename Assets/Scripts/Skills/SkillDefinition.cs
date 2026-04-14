using UnityEngine;

public enum SkillId
{
    Cooking = 0,
    Farming = 1,
    Electricity = 2,
    Crafting = 3,
    Medicine = 4,
    Science = 5,
    Combat = 6,
    Athletics = 7
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

    [SerializeField] private LocalizationItem localizeItem;
    public LocalizationItem LocalizeItem => localizeItem;

    public const int maxSkillLevel = 10;
}