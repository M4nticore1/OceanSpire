using System;
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
    public SkillInstanceData[] Skills;

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
}