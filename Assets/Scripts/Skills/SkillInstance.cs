using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SkillInstance : ILocalizable
{
    public SkillDefinition SkillDefinition { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public float CurrentXp { get; private set; } = 0f;

    public event Action<SkillInstance> OnXpChanged;
    public event Action<SkillInstance> OnLevelChanged;

    public SkillInstance(SkillDefinition definition)
    {
        SkillDefinition = definition;
    }

    public void AddXp(float deltaTime)
    {
        CurrentXp += SkillDefinition.XpGainRate * deltaTime;
        OnXpChanged?.Invoke(this);

        //if (ShouldLevelUp()) {
        //    LevelUp();
        //    ResetCurrentExperience();
        //}
    }

    public void LevelUp()
    {
        SetLevel(CurrentLevel + 1);
    }

    public void SetLevel(int value)
    {
        CurrentLevel = math.clamp(value, 1, SkillDefinition.maxSkillLevel);

        OnLevelChanged?.Invoke(this);
    }

    public void SetXp(float value)
    {
        CurrentXp = value;
    }

    public float GetBonus()
    {
        float bonus = SkillDefinition.BonusPerLevel * (CurrentLevel - 1);
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
        CurrentXp = 0f;
    }

    private bool ShouldLevelUp()
    {
        return CurrentXp >= 1f;
    }
}