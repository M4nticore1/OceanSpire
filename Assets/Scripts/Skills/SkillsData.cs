using System;
using UnityEngine;

[Serializable]
public class SkillInstanceData
{
    public int Id = 0;
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
        int count = skillsComponent.Skills.Count;
        SkillInstanceData[] skills = new SkillInstanceData[count];

        for (int i = 0; i < count; i++) {
            SkillId skillId = (SkillId)Enum.GetValues(typeof(SkillId)).GetValue(i);

            int id = (int)skillsComponent.GetSkill(skillId).skillDefinition.SkillId;
            int level = skillsComponent.GetSkill(skillId).currentLevel;
            float xp = skillsComponent.GetSkill(skillId).currentXp;

            SkillInstanceData data = new SkillInstanceData()
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