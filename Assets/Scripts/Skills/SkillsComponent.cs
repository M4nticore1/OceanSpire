using System.Collections.Generic;
using UnityEngine;

public class SkillsComponent : MonoBehaviour
{
    private Dictionary<SkillId, SkillInstance> skills = new();
    public IReadOnlyDictionary<SkillId, SkillInstance> Skills => skills;

    public void Init(SkillsData data)
    {
        foreach (var saved in data.Skills) { 
            var def = SkillsList.Instance.GetSkillDefinition((SkillId)saved.Id);

            SkillId key = def.SkillId;
            SkillInstance skill = new SkillInstance(def);

            skill.SetLevel(saved.Level);
            skill.SetXp(saved.Xp);

            skills.Add(key, skill);
        }
    }

    public void TryAddExperience(SkillId id, float deltaTime = 1)
    {
        GetSkill(id).AddExperience(deltaTime);
    }

    public SkillInstance GetSkill(SkillId id)
    {
        return skills[id];
    }
}