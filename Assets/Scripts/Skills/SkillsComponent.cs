using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillsComponent : MonoBehaviour
{
    private Dictionary<SkillId, SkillInstance> skills = new();
    public IReadOnlyDictionary<SkillId, SkillInstance> Skills => skills;

    public event Action<SkillInstance, float> OnSkillXpChanged;
    public event Action<SkillInstance, int> OnSkillLevelChanged;

    private void OnDisable()
    {
        foreach (var skill in skills.Values) {
            skill.OnXpChanged -= OnXpChanged;
            skill.OnLevelChanged -= OnLevelChanged;
        }
    }

    public void Init()
    {
        Init(SkillsData.Default() ?? new SkillsData());
    }

    public void Init(SkillsData skillsData)
    {
        if (skillsData == null) {
            Debug.Log("data is not valid");
            Init();
            return;
        }

        foreach (var saved in skillsData.Skills) { 
            var def = SkillsList.Instance.GetSkillDefinition(saved.Id);

            var skillId = def.SkillId;
            var skill = new SkillInstance(def);

            AddSkill(skill);

            skill.SetXp(saved.Xp);
            skill.SetLevel(saved.Level);
        }
    }

    public void AddExperience(SkillId id, float xp)
    {
        GetSkill(id).AddXp(xp);
    }

    public SkillInstance GetSkill(SkillId id)
    {
        return skills[id];
    }

    private void AddSkill(SkillInstance skill)
    {
        var skillId = skill.SkillDefinition.SkillId;
        skills.Add(skillId, skill);

        skill.OnXpChanged += OnXpChanged;
        skill.OnLevelChanged += OnLevelChanged;
    }

    private void OnXpChanged(SkillInstance skill, float xp)
    {
        OnSkillXpChanged?.Invoke(skill, xp);
    }

    private void OnLevelChanged(SkillInstance skill, int level)
    {
        OnSkillLevelChanged?.Invoke(skill, level);
    }
}