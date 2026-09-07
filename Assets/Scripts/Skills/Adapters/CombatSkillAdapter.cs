using UnityEngine;

public class CombatSkillAdapter : SkillAdapter
{
    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        AttackComponent.OnInited += HandleCombatComponentInited;
        AttackComponent.OnDestroyed += HandleCombatComponentDestroyed;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        AttackComponent.OnInited -= HandleCombatComponentInited;
        AttackComponent.OnDestroyed -= HandleCombatComponentDestroyed;

        return true;
    }

    protected override ILevelBonusable GetBonusTarget(SkillsComponent skillsComponent)
    {
        if (skillsComponent == null) return null;

        var combatComponent = skillsComponent.GetComponent<AttackComponent>();

        return combatComponent;
    }

    private void HandleCombatComponentInited(AttackComponent attackComponent)
    {
        if (attackComponent == null) return;

        var skillsComponent = attackComponent.GetComponent<SkillsComponent>();
        if (skillsComponent == null) return;

        AddSkillsComponent(skillsComponent);
    }

    private void HandleCombatComponentDestroyed(AttackComponent attackComponent)
    {
        if (attackComponent == null) return;

        var skillsComponent = attackComponent.GetComponent<SkillsComponent>();
        if (skillsComponent == null) return;

        RemoveSkillsComponent(skillsComponent);
    }
}