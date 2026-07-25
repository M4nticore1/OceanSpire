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
            int levelIndex = OwnedBuilding.LevelComponent.Level - 1;
            if (levelIndex >= 0 && levelIndex < ProductionLevelsData.Length)
                return ProductionLevelsData[levelIndex];

            Debug.LogError($"Invalid level {OwnedBuilding.LevelComponent.Level}", this);
            return null;
        }
    }

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    [SerializeField] private SkillId skillId;
    public SkillId SkillId => skillId;

    public List<CraftItemInstance> CraftItems { get; private set; } = new();
    public CraftItemInstance SelectedCraftItem { get; private set; }

    public float CraftingSpeedBonus { get; private set; } = 0f;
    private CityStorage cityStorage => CityStorage.Instance;

    public event Action OnClicked;
    public event Action<CraftItemInstance> OnItemCraftStarted;
    public event Action<CraftItemInstance> OnItemCraftEnded;
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
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();
        OwnedBuilding.OnClicked -= HandleBuildingClicked;
    }

    public void Init(CraftingModuleData craftingModuleData = null)
    {
        craftingModuleData ??= CraftingModuleData.Default() ?? new CraftingModuleData();

        CreateCraftItems();
        SetCraftingItemByIndex(craftingModuleData.CurrentCraftId);

        if (SelectedCraftItem == null) {
            SetCraftingItemByIndex(0);
        }

        if (craftingModuleData.SelectedCraft != null) {
            SetCraftingFinishTime(craftingModuleData.SelectedCraft.CraftingFinishTime);
        }

        TryCraftItem();
    }

    public void Tick()
    {
        //Debug.Log(IsWorking);
        //Debug.Log(SelectedCraftItem.FinishTime);
        //Debug.Log(SelectedCraftItem.GetRemainingCraftingTime());
        //Debug.Log(SelectedCraftItem.IsCraftingFinished());

        if (IsWorking) {
            UpdateCraftingTime();
            TryCraftItem();
        }
    }

    protected override void OnWorkingStart()
    {
        base.OnWorkingStart();

        // Запуск нового крафта с нуля
        if (SelectedCraftItem != null && SelectedCraftItem.FinishTime == null) {
            if (TryConsumeResources()) {
                SetCraftingTime(0);
                ResetFinishTime();
            }
        }
    }

    protected override void OnWorkingStop()
    {
        base.OnWorkingStop();

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

        // Если уже идет — разрешаем работать
        if (SelectedCraftItem.FinishTime != null) return true;

        // Если еще не начат — проверяем ресурсы
        return IsEnoughResources(SelectedCraftItem.Definition);
    }

    protected override bool ShouldStopWorking()
    {
        if (base.ShouldStopWorking()) return true;

        if (SelectedCraftItem == null) return true;
        if (SelectedCraftItem.IsCraftingFinished()) return true;

        return false;
    }

    public void SetCraftingItem(CraftItemInstance craftItem)
    {
        if (craftItem == null || !CraftItems.Contains(craftItem)) return;
        SelectedCraftItem = craftItem;
    }

    public void SetCraftingItemByIndex(int index)
    {
        if (index < 0 || index >= CraftItems.Count) return;
        SetCraftingItem(CraftItems[index]);
    }

    public void RemoveCraftignItem()
    {
        SelectedCraftItem = null;
    }

    public void UpdateCraftingTime()
    {
        SelectedCraftItem?.UpdateCraftingTime();
    }

    public void SetCraftingTime(int time)
    {
        SelectedCraftItem?.SetCraftingTime(time);
    }

    public void ResetFinishTime()
    {
        SelectedCraftItem?.ResetFinishTime();
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

    public void SetCraftingSpeedBonus(float bonus)
    {
        CraftingSpeedBonus = bonus;

        foreach (var item in CraftItems) {
            item.SetSpeedBonus(bonus);
        }
    }

    private void HandleBuildingClicked()
    {
        if (TryCollectItem()) {
            SetCraftingTime(0);
            SetCraftingFinishTime(null);
            TryStartWorking(); // Пробуем запустить новый цикл крафта
        }

        OnClicked?.Invoke();
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

        cityStorage.Inventory.AddItem(craftItem.Definition.ItemId, craftItem.Amount);

        OnItemCollected?.Invoke(SelectedCraftItem);
        OnModuleItemCollected?.Invoke(this, SelectedCraftItem);
    }

    private bool ShouldCollectItem()
    {
        if (SelectedCraftItem == null) return false;
        if (!SelectedCraftItem.IsCraftingFinished()) return false;

        var storageItem = cityStorage.Inventory.GetItem(SelectedCraftItem.Definition.ProduceItem.Definition.ItemId);
        if (storageItem.Stack.GetItemAmountsSum() >= storageItem.Stack.Amount) return false;

        return true;
    }

    public bool TryConsumeResources()
    {
        if (!ShouldConsumeResources()) return false;

        foreach (var resource in SelectedCraftItem.Definition.ConsumeResources) {
            cityStorage.Inventory.RemoveItem(resource.Definition.ItemId, resource.Amount);
        }

        return true;
    }

    private bool ShouldConsumeResources()
    {
        if (!cityStorage || !IsWorking || SelectedCraftItem == null) return false;
        if (SelectedCraftItem.FinishTime != null) return false; // Уже списаны

        return IsEnoughResources(SelectedCraftItem.Definition);
    }

    public bool TryRefundResources()
    {
        if (!ShouldRefundResources()) return false;

        foreach (var resource in SelectedCraftItem.Definition.ConsumeResources) {
            cityStorage.Inventory.AddItem(resource.Definition.ItemId, resource.Amount);
        }

        SetCraftingFinishTime(null);
        return true;
    }

    private bool ShouldRefundResources()
    {
        if (!cityStorage || SelectedCraftItem == null) return false;

        // Возвращаем ТОЛЬКО если ресурсы были списаны (FinishTime != null) и предмет еще НЕ готов
        return SelectedCraftItem.FinishTime != null && !SelectedCraftItem.IsCraftingFinished();
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

        TryStopWorking();

        OnItemCraftEnded?.Invoke(SelectedCraftItem);
        OnModuleItemCraftEnded?.Invoke(this, SelectedCraftItem);

        return true;
    }

    private bool IsEnoughResources(CraftItemDefinition craftItemDefinition)
    {
        if (!cityStorage || !craftItemDefinition) return false;

        foreach (var resource in craftItemDefinition.ConsumeResources) {
            var storageAmount = cityStorage.Inventory.GetItem(resource.Definition.ItemId).Amount;
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
        var items = new List<ItemInstance>();
        if (SelectedCraftItem == null || !cityStorage) return items;

        foreach (var consumeItem in SelectedCraftItem.Definition.ConsumeResources) {
            var cityAmount = cityStorage.Inventory.GetItem(consumeItem.Definition.ItemId).Amount;
            var amount = Mathf.Min(cityAmount, consumeItem.Amount);
            if (amount <= 0) continue;

            var item = new ItemInstance(consumeItem.Definition);
            item.SetAmount(amount);
            items.Add(item);
        }

        return items;
    }

    public bool CanBeRaided()
    {
        return OwnedBuilding.Definition.IsRaidable;
    }

    private void RegisterModule()
    {
        CraftingModulesManager.Instance?.RegisterCraftingModule(this);
    }

    private void UnregisterModule()
    {
        CraftingModulesManager.Instance?.UnregisterCraftingModule(this);
    }
}