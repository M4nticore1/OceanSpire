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

    protected abstract void OnSkillLevelChanged(SkillsComponent skillsComponent);

    private void OnSkillLevelChanged(SkillInstance skill)
    {
        foreach (var component in skillComponents) {
            if (component.GetSkill(SkillId) != skill) continue;

            OnSkillLevelChanged(component);
            break;
        }
    }

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

    public SkillInstance[] GetSkills()
    {
        var skills = new List<SkillInstance>();
        foreach (var component in skillComponents) {
            var skill = component.GetSkill(skillId);
            skills.Add(skill);
        }

        return skills.ToArray();
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
}