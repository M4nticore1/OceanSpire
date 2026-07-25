using UnityEngine;

public class CraftingModuleSkillAdapter : SkillAdapter
{
    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        WorkComponent.OnComponentCurrentWorkerAdded += OnCurrentWorkerAdded;
        WorkComponent.OnComponentCurrentWorkerRemoved += OnCurrentWorkerRemoved;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        WorkComponent.OnComponentCurrentWorkerAdded -= OnCurrentWorkerAdded;
        WorkComponent.OnComponentCurrentWorkerRemoved -= OnCurrentWorkerRemoved;

        return true;
    }

    protected override void OnSkillLevelChanged(SkillsComponent skillsComponent)
    {
        var interactComponent = skillsComponent.GetComponent<CreatureInteractComponent>();
        var interactBuilding = interactComponent.InteractBuilding;
        if (!interactBuilding) return;

        var craftModule = interactBuilding.GetComponent<CraftingModule>();
        if (!craftModule) return;

        var skill = skillsComponent.GetSkill(SkillId);
        var skillLevel = skill.CurrentLevel;
        var skillBonus = skill.GetBonus();

        var bonusPerLevel = skill.SkillDefinition.BonusPerLevel;
        var skillLastBonus = skillBonus - bonusPerLevel;

        RemoveBonus(craftModule, skillLastBonus);
        AddBonus(craftModule, skillBonus);
    }

    private void AddBonus(CraftingModule module, float bonus)
    {
        var currentBonus = module.CraftingSpeedBonus;
        var finalBonus = currentBonus + bonus;

        module.SetCraftingSpeedBonus(finalBonus);
    }

    private void RemoveBonus(CraftingModule module, float bonus)
    {
        var currentBonus = module.CraftingSpeedBonus;
        var finalBonus = currentBonus - bonus;

        module.SetCraftingSpeedBonus(finalBonus);
    }

    private void OnCurrentWorkerAdded(WorkComponent workComponent, Citizen citizen)
    {
        var craftingModule = workComponent.GetComponent<CraftingModule>();
        if (!craftingModule) return;

        if (craftingModule.SkillId != SkillId) return;

        var skillsComponent = citizen.GetComponent<SkillsComponent>();
        AddBonus(craftingModule, GetBonus(skillsComponent));
        AddSkillsComponent(skillsComponent);
    }

    private void OnCurrentWorkerRemoved(WorkComponent workComponent, Citizen citizen)
    {
        var craftingModule = workComponent.GetComponent<CraftingModule>();
        if (!craftingModule) return;

        if (craftingModule.SkillId != SkillId) return;

        var skillsComponent = citizen.GetComponent<SkillsComponent>();
        RemoveBonus(craftingModule, GetBonus(skillsComponent));
        RemoveSkillsComponent(skillsComponent);
    }
}