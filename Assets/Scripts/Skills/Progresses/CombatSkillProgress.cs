using UnityEngine;

public class CombatSkillProgress : SkillProgress
{
    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        AttackComponent.OnGlobalAttacked += HandleAttacked;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        AttackComponent.OnGlobalAttacked -= HandleAttacked;

        return true;
    }

    private void HandleAttacked(AttackComponent component)
    {
        if (component == null) return;

        var skillsComponent = component.GetComponent<SkillsComponent>();
        if (skillsComponent == null) return;

        AddXp(skillsComponent, component.GetDamage() * XpGain);
    }
}