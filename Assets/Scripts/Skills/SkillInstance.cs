using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SkillInstance : ILocalizable
{
    public SkillDefinition SkillDefinition { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public float CurrentXp { get; private set; } = 0f;

    public event Action<SkillInstance, float> OnXpChanged;
    public event Action<SkillInstance, int> OnLevelChanged;

    public SkillInstance(SkillDefinition definition)
    {
        SkillDefinition = definition;
    }

    public void AddXp(float xp)
    {
        CurrentXp += xp;
        OnXpChanged?.Invoke(this, CurrentXp);
    }

    public void TryLevelUp()
    {
        if (CurrentXp < 1f) return;

        LevelUp();
    }

    public void LevelUp()
    {
        ResetCurrentXp();
        SetLevel(CurrentLevel + 1);
    }

    public void SetLevel(int value)
    {
        CurrentLevel = math.clamp(value, 1, SkillDefinition.MaxSkillLevel + 1);
        OnLevelChanged?.Invoke(this, CurrentLevel);
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

    public bool ShouldLevelUp()
    {
        if (CurrentXp < 1f) return false;
        if (CurrentLevel >= SkillDefinition.MaxSkillLevel) return false;

        return true;
    }

    private void ResetCurrentXp()
    {
        CurrentXp = 0f;
    }
}