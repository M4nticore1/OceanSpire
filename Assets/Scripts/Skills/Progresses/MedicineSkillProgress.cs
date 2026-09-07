using UnityEngine;

public class MedicineSkillProgress : SkillProgress
{
    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        HealthComponent.OnGlobalHealed += HandleHealthHealed;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        HealthComponent.OnGlobalHealed -= HandleHealthHealed;

        return true;
    }

    private void HandleHealthHealed(HealthComponent component, float health)
    {
        if (component == null) return;

        var skillsComponent = component.GetComponent<SkillsComponent>();
        if (skillsComponent == null) return;

        AddXp(skillsComponent, health * XpGain);
    }
}