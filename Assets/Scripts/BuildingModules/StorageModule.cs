using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("BuildingModules/Storage Building Module")]
public class StorageModule : BuildingModule, IRaidable
{
    public StorageModuleLevelData LastStorageLevelData => LastLevelData as StorageModuleLevelData;
    public StorageModuleLevelData StorageLevelData => LevelData as StorageModuleLevelData;

    private bool IsLimitAdded = false;
    private Coroutine addAndRemoveLimitCoroutine;

    private CityStorage cityStorage => CityStorage.Instance;

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.ConstructionComponent.OnConstructionFinished += OnConstructionFinished;
        OwnedBuilding.UpgradeComponent.OnUpgradeFinished += OnUpgradeFinished;
        OwnedBuilding.OnDemolished += OnDemolished;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.ConstructionComponent.OnConstructionFinished -= OnConstructionFinished;
        OwnedBuilding.UpgradeComponent.OnUpgradeFinished -= OnUpgradeFinished;
        OwnedBuilding.OnDemolished -= OnDemolished;
    }

    protected override void OnInit()
    {
        base.OnInit();

        if (OwnedBuilding.ConstructionComponent.GetUnderConstruction() && !OwnedBuilding.UpgradeComponent.IsUnderUpgrade) return;

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

            var definition = cityItem.Definition;
            var item = definition.CreateInstance();
            item.SetAmount(amount);
            items.Add(item);
        }

        return items;
    }

    private void OnConstructionFinished()
    {
        RunAddAndRemoveLimitCoroutine();
    }

    private void OnUpgradeFinished()
    {
        RunAddAndRemoveLimitCoroutine();
    }

    private void OnDemolished()
    {
        RemoveLimit(StorageLevelData);
    }

    private void RunAddAndRemoveLimitCoroutine()
    {
        if (addAndRemoveLimitCoroutine == null) {
            addAndRemoveLimitCoroutine = StartCoroutine(AddAndRemoveLimitCoroutine());
        }
    }

    private void AddAndRemoveLimit()
    {
        AddLimit(StorageLevelData);

        if (LastStorageLevelData) {
            RemoveLimit(LastStorageLevelData);
        }
    }

    private void AddLimit(StorageModuleLevelData levelData)
    {
        if (!levelData) return;
        if (IsLimitAdded) return;
        if (!cityStorage) return;

        foreach (var stack in levelData.Stacks) {
            cityStorage.Inventory.AddLimit(stack.StackEnum, stack.Amount);
        }

        IsLimitAdded = true;
    }

    private void RemoveLimit(StorageModuleLevelData levelData)
    {
        if (!levelData) return;
        if (!IsLimitAdded) return;
        if (!cityStorage) return;

        foreach (var stack in levelData.Stacks) {
            cityStorage.Inventory.RemoveLimit(stack.StackEnum, stack.Amount);
        }

        IsLimitAdded = false;
    }

    private IEnumerator AddAndRemoveLimitCoroutine()
    {
        yield return new WaitForEndOfFrame();

        addAndRemoveLimitCoroutine = null;
        AddAndRemoveLimit();
    }
}