using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("BuildingModules/Storage Building Module")]
public class StorageModule : BuildingModule, IRaidable
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

    public List<ItemInstance> GetRaidLoot()
    {
        var items = new List<ItemInstance>();

        var cityStorage = CityStorage.Instance;
        if (!cityStorage) return items;

        var levelStacksMap = new Dictionary<ItemStackEnum, ItemStack>();
        foreach (var stack in StorageLevelData.Stacks) {
            if (stack != null && !levelStacksMap.ContainsKey(stack.StackEnum)) {
                levelStacksMap.Add(stack.StackEnum, stack);
            }
        }

        foreach (var cityItem in cityStorage.Inventory.Items) {
            if (cityItem.Stack == null) continue;

            if (!levelStacksMap.TryGetValue(cityItem.Stack.StackEnum, out var levelStack)) continue;

            var cityAmount = cityItem.Amount;
            if (cityAmount <= 0) continue;

            var targetAmount = (int)(levelStack.Amount * StorageLevelData.RaidLossRate);
            var amount = Mathf.Min(cityAmount, targetAmount);

            if (amount <= 0) continue;

            var item = new ItemInstance(cityItem.Definition);
            item.SetAmount(amount);
            items.Add(item);
        }

        return items;
    }

    public bool CanBeRaided()
    {
        if (!OwnedBuilding.Definition.IsRaidable) return false;

        return true;
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
