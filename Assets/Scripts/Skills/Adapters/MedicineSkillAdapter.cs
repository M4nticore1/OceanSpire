using UnityEngine;

public class MedicineSkillAdapter : SkillAdapter
{
    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        HealthComponent.OnGlobalInited += HandleHealthComponentInited;
        HealthComponent.OnGlobalInited += HandleHealthComponentDestroyed;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        HealthComponent.OnGlobalInited -= HandleHealthComponentInited;
        HealthComponent.OnGlobalInited -= HandleHealthComponentDestroyed;

        return true;
    }

    protected override ILevelBonusable GetBonusTarget(SkillsComponent skillsComponent)
    {
        if (skillsComponent == null) return null;

        var healthComponent = skillsComponent.GetComponent<HealthComponent>();
        if (healthComponent == null) return null;

        return healthComponent;
    }

    private void HandleHealthComponentInited(HealthComponent component)
    {
        if (component == null) return;

        var skillsComponent = component.GetComponent<SkillsComponent>();
        if (skillsComponent == null) return;

        AddSkillsComponent(skillsComponent);
    }

    private void HandleHealthComponentDestroyed(HealthComponent component)
    {
        if (component == null) return;

        var skillsComponent = component.GetComponent<SkillsComponent>();
        if (skillsComponent == null) return;

        RemoveSkillsComponent(skillsComponent);
    }
}