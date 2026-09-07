using System.Collections.Generic;
using UnityEngine;

public abstract class SkillAdapter : MonoBehaviour
{
    [SerializeField] private SkillId skillId;
    public SkillId SkillId => skillId;

    public List<SkillsComponent> SkillComponents { get; private set; } = new();

    private bool isSubscribed = false;

    private void Awake()
    {
        if (TrySubscribe()) {
            isSubscribed = true;
        }
    }

    private void OnDestroy()
    {
        if (TryUnsubscribe()) {
            isSubscribed = false;
        }

        for (int i = SkillComponents.Count - 1; i >= 0; i--) {
            var component = SkillComponents[i];
            if (component == null) {
                SkillComponents.RemoveAt(i);
                continue;
            }

            component.OnSkillLevelChanged -= OnSkillLevelChanged;
        }
    }

    private void Start()
    {
        if (TrySubscribe()) {
            isSubscribed = true;
        }
    }

    protected virtual bool TrySubscribe()
    {
        if (isSubscribed) return false;

        return true;
    }

    protected virtual bool TryUnsubscribe()
    {
        if (!isSubscribed)
            return false;

        return true;
    }

    protected abstract ILevelBonusable GetBonusTarget(SkillsComponent skillsComponent);

    public SkillInstance[] GetSkills()
    {
        var skills = new List<SkillInstance>();
        foreach (var component in SkillComponents) {
            var skill = component.GetSkill(skillId);
            skills.Add(skill);
        }

        return skills.ToArray();
    }

    protected void AddSkillsComponent(SkillsComponent skillsComponent)
    {
        if (skillsComponent == null)
            return;

        if (SkillComponents.Contains(skillsComponent))
            return;

        SkillComponents.Add(skillsComponent);
        skillsComponent.OnSkillLevelChanged += OnSkillLevelChanged;
    }

    protected void RemoveSkillsComponent(SkillsComponent skillsComponent)
    {
        if (skillsComponent == null)
            return;

        if (!SkillComponents.Contains(skillsComponent))
            return;

        SkillComponents.Remove(skillsComponent);
        skillsComponent.OnSkillLevelChanged -= OnSkillLevelChanged;
    }

    protected void SetSkillId(SkillId skillId)
    {
        this.skillId = skillId;
    }

    // Events
    private void OnSkillLevelChanged(SkillsComponent skillsComponent, SkillInstance skill)
    {
        if (skillsComponent == null)
            return;

        if (skill == null)
            return;

        var bonusTarget = GetBonusTarget(skillsComponent);
        if (bonusTarget == null)
            return;

        var skillBonus = skill.GetBonus();
        var bonusPerLevel = skill.SkillDefinition.BonusPerLevel;
        var lastSkillBonus = skillBonus - bonusPerLevel;

        bonusTarget.SetLevelBonus(bonusTarget.LevelBonus - lastSkillBonus);
        bonusTarget.SetLevelBonus(bonusTarget.LevelBonus + skillBonus);
    }

    protected float GetBonus(SkillsComponent skillsComponent)
    {
        if (skillsComponent == null)
            return 0f;

        var skill = skillsComponent.GetSkill(SkillId);
        var bonus = skill.GetBonus();

        return bonus;
    }

    protected float GetBonusSum()
    {
        var bonus = 0f;
        foreach (var component in SkillComponents) {
            bonus += component.GetSkill(SkillId).GetBonus();
        }

        return bonus;
    }
}