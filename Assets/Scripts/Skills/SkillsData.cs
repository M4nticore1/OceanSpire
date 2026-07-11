using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillInstanceData
{
    public SkillId Id = 0;
    public int Level = 0;
    public float Xp = 0;
}

[Serializable]
public class SkillsData
{
    public SkillInstanceData[] Skills = new SkillInstanceData[0];

    public static SkillsData Default()
    {
        return new SkillsData();
    }

    public static SkillsData Create(SkillsComponent skillsComponent)
    {
        var count = skillsComponent.Skills.Count;
        var skills = new SkillInstanceData[count];

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

            skills[i] = data;
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
        skillsData.Skills = new SkillInstanceData[skillsCount];

        for (int i = 0; i < skillsCount; i++) {
            var skill = new SkillInstanceData();
            skill.Id = (SkillId)Enum.GetValues(typeof(SkillId)).GetValue(i);
            skillsData.Skills[i] = skill;
        }

        return skillsData;
    }

    public static int GetLevelsCountByGameStage()
    {
        int skillsCount = SkillsList.Instance.SkillDefinitionsDict.Count;
        int maxLevelsCount = skillsCount * SkillDefinition.MaxSkillLevel;
        int levelCount = (int)(maxLevelsCount * GameStageSystem.CalculateGameStagePercent());

        return levelCount;
    }
}