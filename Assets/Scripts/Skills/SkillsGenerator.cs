using System.Collections.Generic;
using UnityEngine;

public static class SkillsGenerator
{
    public static SkillsData GetRandomSkillData(int maxLevel = SkillDefinition.maxSkillLevel / 2, float maxLevelBias = 0.5f)
    {
        List<SkillInstance> skills = new List<SkillInstance>();

        foreach (var def in SkillsList.Instance.SkillDefinitionsDict.Values) {
            float t = Random.value;
            float biased = Mathf.Lerp(t, 1f, maxLevelBias);
            int level = Mathf.RoundToInt(biased * maxLevel);

            SkillInstance skill = new SkillInstance(def);
            skill.SetLevel(level);

            skills.Add(skill);
        }

        SkillsData data = new SkillsData(skills);
        return data;
    }
}