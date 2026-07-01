using UnityEngine;

[AddComponentMenu("BuildingModules/Storage Building Module")]
public class StorageModule : BuildingModule
{
    public StorageModuleLevelData StorageLevelData => LevelData as StorageModuleLevelData;

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.UpgradeComponent.OnUpgradeFinished += OnUpgradeCompleted;
        OwnedBuilding.OnDemolished += OnDemolished;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.UpgradeComponent.OnUpgradeFinished -= OnUpgradeCompleted;
        OwnedBuilding.OnDemolished -= OnDemolished;
    }

    protected override void OnInit()
    {
        base.OnInit();

        if (OwnedBuilding.UpgradeComponent.NextLevel == 1 && OwnedBuilding.ConstructionComponent.GetUnderConstruction()) return;

        AddLimit(StorageLevelData);
    }

    private void OnUpgradeCompleted()
    {
        RemoveLimit(LevelsData[OwnedBuilding.LevelComponent.Level - 1] as StorageModuleLevelData);
        AddLimit(StorageLevelData);
    }

    private void OnDemolished()
    {
        RemoveLimit(StorageLevelData);
    }

    private void AddLimit(StorageModuleLevelData levelData)
    {
        foreach (var stack in levelData.Stacks) {
            CityStorage.Instance.Inventory.AddLimit(stack.StackEnum, stack.Amount);
        }
    }

    private void RemoveLimit(StorageModuleLevelData levelData)
    {
        if (!IsInited) return;

        foreach (var stack in levelData.Stacks) {
            CityStorage.Instance.Inventory.RemoveLimit(stack.StackEnum, stack.Amount);
        }
    }
}
