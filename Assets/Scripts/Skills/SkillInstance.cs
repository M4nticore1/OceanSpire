using Unity.Mathematics;
using UnityEngine;

public class SkillInstance
{
    public SkillDefinition skillDefinition { get; private set; }
    public int currentLevel { get; private set; } = 1;
    public float currentXp { get; private set; } = 0f;

    public SkillInstance(SkillDefinition definition)
    {
        skillDefinition = definition;
    }

    public void AddExperience(float deltaTime)
    {
        currentXp += skillDefinition.XpGainRate * deltaTime;

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
        float bonus = skillDefinition.BonusPerLevel * (currentLevel - 1);
        return bonus;
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