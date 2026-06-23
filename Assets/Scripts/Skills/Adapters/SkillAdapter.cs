using System.Collections.Generic;
using UnityEngine;

public abstract class SkillAdapter : MonoBehaviour
{
    [SerializeField] private SkillId skillId;
    protected SkillId SkillId => skillId;

    private List<SkillsComponent> skillComponents = new();

    private bool isSubscribed = false;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void OnDestroy()
    {
        foreach (var component in skillComponents) {
            component.GetSkill(skillId).OnLevelChanged -= OnSkillLevelChanged;
        }
    }

    private void Start()
    {
        TrySubscribe();
    }

    protected abstract void AddBonus(float bonus);

    protected abstract void RemoveBonus(float bonus);

    protected virtual bool TrySubscribe()
    {
        if (isSubscribed) return false;

        isSubscribed = true;
        return true;
    }

    protected virtual bool TryUnsubscribe()
    {
        if (!isSubscribed) return false;

        isSubscribed = false;
        return true;
    }

    protected void AddSkillsComponent(SkillsComponent skillsComponent)
    {
        if (!skillsComponent) {
            Debug.LogError("skillsComponent is not valid");
            return;
        }

        skillComponents.Add(skillsComponent);

        var skill = skillsComponent.GetSkill(skillId);
        skill.OnLevelChanged += OnSkillLevelChanged;
    }

    protected void RemoveSkillsComponent(SkillsComponent skillsComponent)
    {
        if (!skillComponents.Contains(skillsComponent)) {
            Debug.LogError("skillComponents does not contain skillsComponent");
            return;
        }

        skillComponents.Remove(skillsComponent);

        var skill = skillsComponent.GetSkill(skillId);
        skill.OnLevelChanged -= OnSkillLevelChanged;
    }

    protected void SetSkillId(SkillId skillId)
    {
        this.skillId = skillId;
    }

    protected float GetBonus(SkillsComponent skillsComponent)
    {
        var skill = skillsComponent.GetSkill(SkillId);
        var bonus = skill.GetBonus();

        return bonus;
    }

    protected float GetBonusSum()
    {
        var bonus = 0f;
        foreach (var component in skillComponents) {
            bonus += component.GetSkill(SkillId).GetBonus();
        }

        return bonus;
    }

    private void OnSkillLevelChanged(SkillInstance skill)
    {
        var skillLevel = skill.CurrentLevel;
        var skillBonus = skill.GetBonus();

        var skillLastLevel = skillLevel - 1;
        var skillLastBonus = skill.SkillDefinition.BonusPerLevel * skillLastLevel;

        RemoveBonus(skillLastBonus);
        AddBonus(skillBonus);
    }
}