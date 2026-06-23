using UnityEngine;

public class CraftingModuleSkillAdapter : SkillAdapter
{
    [SerializeField] private CraftingModule craftingModule;

    protected override bool TrySubscribe()
    {
        if (!craftingModule.OwnedBuilding) return false;
        if (!base.TrySubscribe()) return false;

        craftingModule.OwnedBuilding.WorkComponent.OnCurrentWorkerAdded += OnCurrentWorkerAdded;
        craftingModule.OwnedBuilding.WorkComponent.OnCurrentWorkerRemoved += OnCurrentWorkerRemoved;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!craftingModule.OwnedBuilding) return false;
        if (!base.TryUnsubscribe()) return false;

        craftingModule.OwnedBuilding.WorkComponent.OnCurrentWorkerAdded -= OnCurrentWorkerAdded;
        craftingModule.OwnedBuilding.WorkComponent.OnCurrentWorkerRemoved -= OnCurrentWorkerRemoved;

        return true;
    }

    protected override void AddBonus(float bonus)
    {
        var currentBonus = craftingModule.CraftingSpeedBonus;
        var skillBonus = bonus;
        var finalBonus = currentBonus + skillBonus;

        craftingModule.SetCraftingSpeedBonus(finalBonus);
    }

    protected override void RemoveBonus(float bonus)
    {
        var currentBonus = craftingModule.CraftingSpeedBonus;
        var skillBonus = bonus;
        var finalBonus = currentBonus - skillBonus;

        craftingModule.SetCraftingSpeedBonus(finalBonus);
    }

    private void OnCurrentWorkerAdded(Citizen citizen)
    {
        var skillsComponent = citizen.GetComponent<SkillsComponent>();
        AddBonus(GetBonus(skillsComponent));
    }

    private void OnCurrentWorkerRemoved(Citizen citizen)
    {
        var skillsComponent = citizen.GetComponent<SkillsComponent>();
        RemoveBonus(GetBonus(skillsComponent));
    }
}