using System;
using System.Collections.Generic;

public class SkillInstanceData
{
    public int id { get; private set; }
    public int level { get; private set; }
    public float xp { get; private set; }

    public SkillInstanceData(int id, int level, float xp)
    {
        this.id = id;
        this.level = level;
        this.xp = xp;
    }
}

[Serializable]
public class SkillsData
{
    public SkillInstanceData[] skills { get; private set; }

    public SkillsData(List<SkillInstance> skills)
    {
        this.skills = new SkillInstanceData[skills.Count];

        for (int i = 0; i < skills.Count; i++) {
            int id = (int)skills[i].skillDefinition.SkillId;
            int level = skills[i].currentLevel;
            float xp = skills[i].currentXp;

            SkillInstanceData data = new SkillInstanceData(id, level, xp);
            this.skills[i] = data;
        }
    }
}