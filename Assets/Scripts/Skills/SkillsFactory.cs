using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SkillsFactory
{
    public static SkillsData CreateRandomSkillsData(int levelsCount)
    {
        Dictionary<int, SkillInstanceData> skills = new();

        foreach(var def in SkillsList.Instance.SkillDefinitionsDict.Values) {
            int id = (int)def.SkillId;

            SkillInstanceData skill = new SkillInstanceData()
            {
                Id = id,
            };

            skills.Add(id, skill);
        }

        int randomCount = Random.Range(levelsCount / 2, levelsCount + 1);
        randomCount = Mathf.Max(randomCount, 1);

        for (int i = 0; i < randomCount; i++) {
            int skillIndex = Random.Range(0, skills.Values.Count);
            skills[skillIndex].Level += 1;
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
        int multiplier = BuildingsManager.Instance.MaxFloorsCount / skillsCount;
        int count = BuildingsManager.Instance.BuiltFloors.Count * multiplier;

        return multiplier;
    }
}