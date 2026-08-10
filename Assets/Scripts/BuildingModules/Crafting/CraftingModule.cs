using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Building Modules/Crafting Module")]
public class CraftingModule : BuildingModule, IElectricible, IRaidable
{
    public ProductionModuleLevelData[] ProductionLevelsData => levelsData.OfType<ProductionModuleLevelData>().ToArray();

    public ProductionModuleLevelData ProductionLevelData
    {
        get {
            if (!OwnedBuilding) return null;
            if (levelsData == null) return null;

            int levelIndex = OwnedBuilding.LevelComponent.Level - 1;
            if (levelIndex >= 0 && levelIndex < levelsData.Length) {
                return levelsData[levelIndex] as ProductionModuleLevelData;
            }

            Debug.LogError($"Invalid level {OwnedBuilding.LevelComponent.Level} on {name}", this);
            return null;
        }
    }

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    public List<CraftItemInstance> CraftItems { get; private set; } = new();
    public CraftItemInstance SelectedCraftItem { get; private set; }

    public float CraftingSpeedMultiplier { get; private set; } = 1f;

    private CityStorage cityStorage => CityStorage.Instance;
    private EnergyShortageManager energyShortageManager => EnergyShortageManager.Instance;

    public event Action OnClicked;
    public event Action<CraftItemInstance> OnItemCraftStarted;
    public event Action<CraftItemInstance> OnItemCraftFinished;
    public event Action<CraftItemInstance> OnItemCollected;
    public event Action<CraftItemInstance> OnItemNotCollected;

    public static event Action<CraftingModule, CraftItemInstance> OnModuleItemCraftStarted;
    public static event Action<CraftingModule, CraftItemInstance> OnModuleItemCraftEnded;
    public static event Action<CraftingModule, CraftItemInstance> OnModuleItemCollected;
    public static event Action<CraftingModule, CraftItemInstance> OnModuleItemNotCollected;

    protected override void OnEnable()
    {
        base.OnEnable();

        RegisterModule();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        UnregisterModule();
    }

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.OnClicked += HandleBuildingClicked;
        cityStorage.Inventory.OnItemAmountChanged += HandleStorageAmountChanged;

        energyShortageManager.OnEnergyShortageStarted += HandleEnergyShortageStarted;
        energyShortageManager.OnEnergyShortageEnded += HandleEnergyShortageEnded;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.OnClicked -= HandleBuildingClicked;
        cityStorage.Inventory.OnItemAmountChanged -= HandleStorageAmountChanged;

        energyShortageManager.OnEnergyShortageStarted -= HandleEnergyShortageStarted;
        energyShortageManager.OnEnergyShortageEnded -= HandleEnergyShortageEnded;
    }

    public void Init()
    {
        Init(CraftingModuleData.Default());
    }

    public void Init(CraftingModuleData craftingModuleData)
    {
        if (craftingModuleData == null) {
            Debug.LogError($"[{nameof(CraftingModule)}] Crafting Module Data is not valid!");
            craftingModuleData = CraftingModuleData.Default();
            return;
        }

        CreateCraftItems();
        SetCraftingItemByIndex(craftingModuleData.CurrentCraftId);

        if (SelectedCraftItem == null && CraftItems.Count > 0) {
            SetCraftingItemByIndex(0);
        }

        if (SelectedCraftItem != null) {
            SelectedCraftItem.SetFinishTime(craftingModuleData.SelectedCraft.CraftingFinishTime);
            SelectedCraftItem.SetCraftingTime(craftingModuleData.SelectedCraft.CurrentCraftingTime);
            SelectedCraftItem.SetResourcesSpent(craftingModuleData.SelectedCraft.ResourcesSpent);
        }

        TryCraftItem();
        TryStartWorking();
    }

    public void Tick()
    {
        var isEnergyShortage = energyShortageManager ? energyShortageManager.IsUnderEnergyShortage && electricityConsumption > 0 : false;

        if (IsWorking && !isEnergyShortage) {
            UpdateCraftingTime();

            if (TryCraftItem()) {
                TryStopWorking();
            }
        }
    }

    protected override void HandleWorkingStart()
    {
        base.HandleWorkingStart();

        TrySpendResources();

        if (SelectedCraftItem.FinishTime == null && !SelectedCraftItem.IsCraftingFinished()) {
            ResetFinishTimeByCraftingTime();
        }

        //if (SelectedCraftItem.FinishTime == null && !SelectedCraftItem.IsCraftingFinished()) {
        //    if (TryConsumeResources()) {
        //        SetCraftingTime(0);
        //        ResetFinishTime();
        //    }
        //    else {
        //        TryStopWorking();
        //    }
        //}
    }

    protected override void HandleWorkingStop()
    {
        base.HandleWorkingStop();

        SetCraftingFinishTime(null);
    }

    protected override void HandleUpgradeFinished()
    {
        if (!TryCollectItem()) {
            TryRefundResources();
        }

        SetCraftingFinishTime(null);
        CreateCraftItems();
        SetCraftingItemByIndex(0);

        base.HandleUpgradeFinished();
    }

    protected override bool ShouldStartWorking()
    {
        if (!base.ShouldStartWorking()) return false;

        if (!cityStorage) return false;
        if (SelectedCraftItem == null) return false;
        if (SelectedCraftItem.IsCraftingFinished()) return false;
        if (!IsEnoughResources(SelectedCraftItem.Definition) && !SelectedCraftItem.IsResourcesSpent) return false;

        return true;
    }

    protected override bool ShouldStopWorking()
    {
        if (base.ShouldStopWorking()) return true;

        if (SelectedCraftItem == null) return true;
        if (SelectedCraftItem.IsCraftingFinished()) return true;
        if (energyShortageManager.IsUnderEnergyShortage && electricityConsumption > 0) return true;
        if (!IsEnoughResources(SelectedCraftItem.Definition)) return true;

        return false;
    }

    public void SetCraftingItemAndApply(CraftItemInstance craftItem)
    {
        if (!TryCollectItem()) {
            TryRefundResources();
        }

        SetCraftingItem(craftItem);
        SetCraftingFinishTime(null);
        SetCraftingTime(0);
        SetResourcesSpent(false);

        if (IsWorking) {
            ResetFinishTimeByCraftingTime();

            if (!TryStopWorking()) {
                TrySpendResources();
            }
        }
        else {
            TryStartWorking();
        }
    }

    public void SetCraftingItem(CraftItemInstance craftItem)
    {
        if (craftItem == null) {
            RemoveCraftignItem();
        }

        if (!CraftItems.Contains(craftItem)) return;

        var text = craftItem != null ? craftItem.Definition.name : "null";
        SelectedCraftItem = craftItem;
    }

    public void SetCraftingItemByIndex(int index)
    {
        if (index < 0) return;
        if (index >= CraftItems.Count) return;

        SetCraftingItem(CraftItems[index]);
    }

    public void RemoveCraftignItem()
    {
        SelectedCraftItem = null;
    }

    public void UpdateCraftingTime()
    {
        SelectedCraftItem?.UpdateCraftingTimeByFinishTime();
    }

    public void SetCraftingTime(int time)
    {
        SelectedCraftItem?.SetCraftingTime(time);
    }

    public void ResetFinishTimeByCraftingTime()
    {
        SelectedCraftItem?.ResetFinishTimeByCurrentCraftingTime();
    }

    public void SetCraftingFinishTime(long? seconds)
    {
        if (SelectedCraftItem == null) return;

        SelectedCraftItem.SetFinishTime(seconds);

        if (seconds != null) {
            OnItemCraftStarted?.Invoke(SelectedCraftItem);
            OnModuleItemCraftStarted?.Invoke(this, SelectedCraftItem);
        }
    }

    public void SetCraftingSpeedBonus(float multiplier)
    {
        CraftingSpeedMultiplier = Mathf.Max(0, multiplier);

        foreach (var item in CraftItems) {
            item.SetCraftingSpeedMultiplier(multiplier);
        }
    }

    public void SetResourcesSpent(bool value)
    {
        SelectedCraftItem?.SetResourcesSpent(value);
    }

    public bool TryCollectItem()
    {
        if (ShouldCollectItem()) {
            CollectItem();
            return true;
        }

        OnItemNotCollected?.Invoke(SelectedCraftItem);
        OnModuleItemNotCollected?.Invoke(this, SelectedCraftItem);
        return false;
    }

    private void CollectItem()
    {
        if (!cityStorage || SelectedCraftItem == null) return;

        var craftItem = SelectedCraftItem.Definition.ProduceItem;
        if (craftItem == null || !craftItem.Definition) return;

        cityStorage.Inventory.AddItemAmount(craftItem.Definition.ItemId, craftItem.Amount);

        OnItemCollected?.Invoke(SelectedCraftItem);
        OnModuleItemCollected?.Invoke(this, SelectedCraftItem);
    }

    private bool ShouldCollectItem()
    {
        if (SelectedCraftItem == null) return false;
        if (!SelectedCraftItem.IsCraftingFinished()) return false;

        var produceItem = SelectedCraftItem.Definition?.ProduceItem;
        if (produceItem == null) return false;
        if (!produceItem.Definition) return false;

        var storageItem = cityStorage.Inventory.GetItem(produceItem.Definition.ItemId);
        if (storageItem != null && storageItem.Stack != null) {
            if (storageItem.Stack.GetItemAmountsSum() >= storageItem.Stack.Amount) return false;
        }

        return true;
    }

    public bool TrySpendResources()
    {
        if (!ShouldSpendResources()) return false;

        return SelectedCraftItem.TrySpendResources();
    }

    private bool ShouldSpendResources()
    {
        if (!cityStorage) return false;
        if (SelectedCraftItem == null) return false;

        return IsEnoughResources(SelectedCraftItem.Definition);
    }

    public bool TryRefundResources()
    {
        if (!ShouldRefundResources()) return false;

        return SelectedCraftItem.TryRefundResources();
    }

    private bool ShouldRefundResources()
    {
        if (!cityStorage) return false;
        if (SelectedCraftItem == null) return false;

        return true;
    }

    private void CreateCraftItems()
    {
        CraftItems.Clear();
        if (!ProductionLevelData) return;

        foreach (var def in ProductionLevelData.CraftItems) {
            var item = def.CreateInstance(CraftItemData.Default());
            if (item != null) CraftItems.Add(item);
        }
    }

    private bool TryCraftItem()
    {
        if (SelectedCraftItem == null) return false;
        if (!SelectedCraftItem.IsCraftingFinished()) return false;

        OnItemCraftFinished?.Invoke(SelectedCraftItem);
        OnModuleItemCraftEnded?.Invoke(this, SelectedCraftItem);

        return true;
    }

    private bool IsEnoughResources(CraftItemDefinition craftItemDefinition)
    {
        if (!craftItemDefinition) return false;
        if (!cityStorage) return false;

        foreach (var resource in craftItemDefinition.ConsumeResources) {
            if (resource == null) continue;
            if (!resource.Definition) continue;

            var item = cityStorage.Inventory.GetItem(resource.Definition.ItemId);
            var storageAmount = item != null ? item.Amount : 0;
            if (resource.Amount > storageAmount) return false;
        }

        return true;
    }

    public bool ShouldSpendElectricity()
    {
        return IsWorking;
    }

    public int GetIndexOfCurrentCraftItem()
    {
        if (CraftItems.Contains(SelectedCraftItem)) {
            return CraftItems.IndexOf(SelectedCraftItem);
        }
        else {
            return 0;
        }
    }

    public float GetElectricityConsumption()
    {
        return ElectricityConsumption;
    }

    public List<ItemInstance> GetRaidLoot()
    {
        if (!cityStorage) return null;
        if (SelectedCraftItem == null) return null;

        var items = new List<ItemInstance>();
        foreach (var consumeItem in SelectedCraftItem.Definition.ConsumeResources) {
            var definition = consumeItem.Definition;
            var cityAmount = cityStorage.Inventory.GetItem(consumeItem.Definition.ItemId).Amount;

            var amount = Mathf.Min(cityAmount, consumeItem.Amount);
            if (amount <= 0) continue;

            var item = definition.CreateInstance();
            if (item == null) {
                Debug.LogError($"[{nameof(CraftingModule)}] Item is not valid!");
                continue;
            }

            item.SetAmount(amount);
            items.Add(item);
        }

        return items;
    }

    private void RegisterModule()
    {
        CraftingModulesManager.Instance?.RegisterCraftingModule(this);
    }

    private void UnregisterModule()
    {
        CraftingModulesManager.Instance?.UnregisterCraftingModule(this);
    }

    private void HandleBuildingClicked()
    {
        if (TryCollectItem()) {
            SetCraftingTime(0);
            SetCraftingFinishTime(null);
            SetResourcesSpent(false);
            TryStartWorking();
        }

        OnClicked?.Invoke();
    }

    private void HandleStorageAmountChanged(ItemInstance item)
    {
        TryStartWorking();
    }

    private void HandleEnergyShortageStarted()
    {
        TryStopWorking();
    }

    private void HandleEnergyShortageEnded()
    {
        TryStartWorking();
    }
}