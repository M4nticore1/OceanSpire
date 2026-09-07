using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillsComponent : MonoBehaviour
{
    private Dictionary<SkillId, SkillInstance> skillsDict = new();
    public IReadOnlyDictionary<SkillId, SkillInstance> SkillsDict => skillsDict;

    public event Action<SkillsComponent, SkillInstance> OnSkillXpChanged;
    public event Action<SkillsComponent, SkillInstance> OnSkillLevelChanged;

    private void OnDisable()
    {
        foreach (var skill in skillsDict.Values) {
            if (skill == null) continue;

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
            Debug.Log($"[{nameof(SkillsComponent)}] SkillsData is not valid");
            Init();
            return;
        }

        foreach (var skill in skillsDict.Values) {
            skill.OnXpChanged -= OnXpChanged;
            skill.OnLevelChanged -= OnLevelChanged;
        }

        skillsDict.Clear();

        var savedSkills = new Dictionary<SkillId, SkillInstanceData>();

        foreach (var saved in skillsData.Skills)
            savedSkills[saved.Id] = saved;

        foreach (var def in SkillsList.Instance.SkillDefinitions) {
            var skill = new SkillInstance(def);

            if (savedSkills.TryGetValue(def.SkillId, out var saved)) {
                skill.SetXp(saved.Xp);
                skill.SetLevel(saved.Level);
            }

            AddSkill(skill);
        }
    }

    public void AddXP(SkillId id, float xp)
    {
        GetSkill(id)?.AddXp(xp);
    }

    public SkillInstance GetSkill(SkillId id)
    {
        skillsDict.TryGetValue(id, out var skill);
        return skill;
    }

    private void AddSkill(SkillInstance skill)
    {
        var skillId = skill.SkillDefinition.SkillId;
        skillsDict.Add(skillId, skill);

        skill.OnXpChanged += OnXpChanged;
        skill.OnLevelChanged += OnLevelChanged;
    }

    private void OnXpChanged(SkillInstance skill, float xp)
    {
        OnSkillXpChanged?.Invoke(this, skill);
    }

    private void OnLevelChanged(SkillInstance skill, int level)
    {
        OnSkillLevelChanged?.Invoke(this, skill);
    }
}