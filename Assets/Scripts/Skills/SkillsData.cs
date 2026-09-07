using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillInstanceData
{
    public SkillId Id = 0;
    public int Level = 1;
    public float Xp = 0;
}

[Serializable]
public class SkillsData
{
    public List<SkillInstanceData> Skills = new();

    public static SkillsData Default()
    {
        return new SkillsData();
    }

    public static SkillsData Create(SkillsComponent skillsComponent)
    {
        var skills = new List<SkillInstanceData>();

        var count = skillsComponent.SkillsDict.Count;
        for (int i = 0; i < count; i++) {
            var skillId = (SkillId)Enum.GetValues(typeof(SkillId)).GetValue(i);

            var skill = skillsComponent.GetSkill(skillId);
            var id = skill.SkillDefinition.SkillId;
            var level = skill.CurrentLevel;
            var xp = skill.CurrentXp;

            var data = new SkillInstanceData()
            {
                Id = id,
                Level = level,
                Xp = xp,
            };

            skills.Add(data);
        }

        return new SkillsData()
        {
            Skills = skills,
        };
    }

    public static SkillsData CreateByLevelsCount(int levelsCount)
    {
        var maxSkillLevel = SkillDefinition.MaxSkillLevel;
        var skillsCount = SkillsList.Instance.SkillDefinitions.Length;
        var maxLevelsCount = maxSkillLevel * skillsCount;
        levelsCount = Mathf.Min(levelsCount, maxLevelsCount);

        var skillsData = CreateFilledSkillsData();
        if (skillsData.Skills == null || skillsData.Skills.Count != skillsCount) {
            Debug.LogError("SkillsData.Skills length mismatch with SkillDefinitions!");
            return skillsData;
        }

        List<int> availableSkillIndices = new List<int>(maxLevelsCount);
        for (int i = 0; i < skillsCount; i++) {
            for (int j = 0; j < maxSkillLevel; j++) {
                availableSkillIndices.Add(i);
            }
        }

        for (int i = 0; i < levelsCount; i++) {
            int randomIndex = UnityEngine.Random.Range(0, availableSkillIndices.Count);
            int chosenSkillId = availableSkillIndices[randomIndex];

            skillsData.Skills[chosenSkillId].Level++;
            availableSkillIndices.RemoveAt(randomIndex);
        }

        return skillsData;
    }

    public static SkillsData CreateFilledSkillsData()
    {
        var skillsData = Default();
        var skillsCount = SkillsList.Instance.SkillDefinitions.Length;
        skillsData.Skills = new();

        for (int i = 0; i < skillsCount; i++) {
            var skill = new SkillInstanceData();
            skill.Id = (SkillId)Enum.GetValues(typeof(SkillId)).GetValue(i);
            skillsData.Skills.Add(skill);
        }

        return skillsData;
    }

    public static int GetLevelsCountByGameStage()
    {
        int skillsCount = SkillsList.Instance.SkillDefinitions.Length;
        int maxLevelsCount = skillsCount * SkillDefinition.MaxSkillLevel;
        int levelCount = (int)(maxLevelsCount * GameStageSystem.CalculateGameStagePercent());

        return levelCount;
    }
}