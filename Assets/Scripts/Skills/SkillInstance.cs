using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SkillInstance : ILocalizable
{
    public SkillDefinition SkillDefinition { get; private set; }
    public int currentLevel { get; private set; } = 1;
    public float currentXp { get; private set; } = 0f;

    public SkillInstance(SkillDefinition definition)
    {
        SkillDefinition = definition;
    }

    public void AddExperience(float deltaTime)
    {
        currentXp += SkillDefinition.XpGainRate * deltaTime;

        if (ShouldLevelUp()) {
            LevelUp();
            ResetCurrentExperience();
        }
    }

    public void LevelUp()
    {
        SetLevel(currentLevel + 1);
    }

    public void SetLevel(int value)
    {
        currentLevel = math.clamp(value, 1, SkillDefinition.maxSkillLevel);
    }

    public void SetXp(float value)
    {
        currentXp = value;
    }

    public float GetBonus()
    {
        float bonus = SkillDefinition.BonusPerLevel * (currentLevel - 1);
        return bonus;
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            {"skillBonus", $"<color=green>{GetBonus() * 100}%</color>"}
        };
    }

    private void ResetCurrentExperience()
    {
        currentXp = 0f;
    }

    private bool ShouldLevelUp()
    {
        return currentXp >= 1f;
    }
}