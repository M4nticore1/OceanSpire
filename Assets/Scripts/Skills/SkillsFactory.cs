using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SkillsFactory
{
    public static SkillsData CreateRandomSkillsData(int levelsCount)
    {
        Dictionary<SkillId, SkillInstanceData> skills = new();

        foreach(var def in SkillsList.Instance.SkillDefinitionsDict.Values) {
            var id = def.SkillId;

            var skill = new SkillInstanceData()
            {
                Id = id,
            };

            skills.Add(id, skill);
        }

        int randomCount = UnityEngine.Random.Range(levelsCount / 2, levelsCount + 1);
        randomCount = Mathf.Max(randomCount, 1);

        for (int i = 0; i < randomCount; i++) {
            var skillIndex = UnityEngine.Random.Range(0, skills.Values.Count);
            var skillid = (SkillId)Enum.GetValues(typeof(SkillId)).GetValue(skillIndex);

            skills[skillid].Level += 1;
        }

        SkillsData data = new SkillsData()
        {
            Skills = skills.Values.ToArray()
        };

        return data;
    }

    public static int GetLevelsCount()
    {
        int skillsCount = SkillsList.Instance.SkillDefinitionsDict.Count;
        int maxLevelsCount = skillsCount * SkillDefinition.maxSkillLevel;
        int levelCount = (int)(maxLevelsCount * GameStageSystem.CalculateGameStagePercent());

        return levelCount;
    }
}