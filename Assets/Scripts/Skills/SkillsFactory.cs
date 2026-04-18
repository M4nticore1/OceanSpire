using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SkillsFactory
{
    public static SkillsData CreateRandomSkillsData(int levelsCount)
    {
        Dictionary<int, SkillInstance> skills = new();

        foreach(var def in SkillsList.Instance.SkillDefinitionsDict.Values) {
            SkillInstance skill = new SkillInstance(def);
            skills.Add((int)skill.skillDefinition.SkillId, skill);
        }

        int randomCount = Random.Range(levelsCount / 2, levelsCount + 1);
        randomCount = Mathf.Max(randomCount, 1);

        for (int i = 0; i < randomCount; i++) {
            int skillIndex = Random.Range(0, SkillsList.Instance.SkillDefinitionsDict.Count);
            SkillDefinition def = SkillsList.Instance.SkillDefinitionsDict.Values.ToArray()[skillIndex];

            skills[skillIndex].LevelUp();
        }

        SkillsData data = new SkillsData(skills.Values.ToList());
        return data;
    }

    public static int GetLevelsCount()
    {
        int skillsCount = SkillsList.Instance.SkillDefinitionsDict.Count;
        int maxLevelsCount = skillsCount * SkillDefinition.maxSkillLevel;
        int multiplier = BuildingsManager.instance.MaxFloorsCount / skillsCount;
        int count = BuildingsManager.instance.BuiltFloors.Count * multiplier;

        return multiplier;
    }
}