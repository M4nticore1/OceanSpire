using UnityEngine;

public class CraftingModuleSkillAdapter : SkillAdapter
{
    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        BuildingInteractorsHandler.OnComponentCurrentInteractorAdded += OnCurrentWorkerAdded;
        BuildingInteractorsHandler.OnComponentCurrentInteractorRemoved += OnCurrentWorkerRemoved;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        BuildingInteractorsHandler.OnComponentCurrentInteractorAdded -= OnCurrentWorkerAdded;
        BuildingInteractorsHandler.OnComponentCurrentInteractorRemoved -= OnCurrentWorkerRemoved;

        return true;
    }

    protected override ILevelBonusable GetBonusTarget(SkillsComponent skillsComponent)
    {
        if (skillsComponent == null) return null;

        var interactComponent = skillsComponent.GetComponent<CreatureInteractComponent>();
        if (interactComponent == null) return null;

        var interactBuilding = interactComponent.InteractBuilding;
        if (interactBuilding == null) return null;

        var craftingModule = interactBuilding.GetComponent<CraftingModule>();

        return craftingModule;
    }

    private void AddBonus(CraftingModule module, float bonus)
    {
        if (module == null) return;

        var currentBonus = module.LevelBonus;
        module.SetLevelBonus(currentBonus + bonus);
    }

    private void RemoveBonus(CraftingModule module, float bonus)
    {
        if (module == null) return;

        var currentBonus = module.LevelBonus;
        module.SetLevelBonus(currentBonus - bonus);
    }

    private void OnCurrentWorkerAdded(BuildingInteractorsHandler workComponent, Human human)
    {
        if (workComponent == null) return;

        var citizen = human as Citizen;
        if (citizen == null) return;

        var craftingModule = workComponent.GetComponent<CraftingModule>();
        if (craftingModule == null) return;

        if (craftingModule.OwnedBuilding.SkillId != SkillId) return;

        var skillsComponent = citizen.GetComponent<SkillsComponent>();
        AddBonus(craftingModule, GetBonus(skillsComponent));
        AddSkillsComponent(skillsComponent);
    }

    private void OnCurrentWorkerRemoved(BuildingInteractorsHandler workComponent, Human human)
    {
        if (workComponent == null) return;

        var citizen = human as Citizen;
        if (citizen == null) return;

        var craftingModule = workComponent.GetComponent<CraftingModule>();
        if (craftingModule == null) return;

        if (craftingModule.OwnedBuilding.SkillId != SkillId) return;

        var skillsComponent = citizen.GetComponent<SkillsComponent>();
        RemoveBonus(craftingModule, GetBonus(skillsComponent));
        RemoveSkillsComponent(skillsComponent);
    }
}