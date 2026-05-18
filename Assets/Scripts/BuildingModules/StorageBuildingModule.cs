using UnityEngine;

[AddComponentMenu("BuildingModules/Storage Building Module")]
public class StorageBuildingModule : BuildingModule
{
    public StorageModuleLevelData StorageLevelData => LevelData as StorageModuleLevelData;

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.onConstructionFinished += OnConstructionFinished;
        OwnedBuilding.onDemolished += OnDemolished;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.onConstructionFinished -= OnConstructionFinished;
        OwnedBuilding.onDemolished -= OnDemolished;
    }

    protected override void OnInited()
    {
        base.OnInited();

        if (OwnedBuilding.ConstructionComponent.IsUnderConstruction) return;

        AddLimit();
    }

    private void OnConstructionFinished()
    {
        AddLimit();
    }

    private void OnDemolished()
    {
        RemoveLimit();
    }

    private void AddLimit()
    {
        foreach (var stack in StorageLevelData.Stacks) {
            CityStorage.Instance.Inventory.AddLimit(stack.StackEnum, stack.Amount);
        }
    }

    private void RemoveLimit()
    {
        if (!IsInited) return;

        foreach (var stack in StorageLevelData.Stacks) {
            CityStorage.Instance.Inventory.RemoveLimit(stack.StackEnum, stack.Amount);
        }
    }
}
