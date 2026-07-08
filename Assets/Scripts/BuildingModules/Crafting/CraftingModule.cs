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

    public void Init()
    {
        Init(CraftingModuleData.Default() ?? new CraftingModuleData());
    }

    public void Init(CraftingModuleData craftingModuleData)
    {
        if (craftingModuleData == null) {
            Debug.LogError("Crafting Module Data is not valid");
            Init();
            return;
        }

        CreateCraftItems();
        SetCraftingItemByIndex(craftingModuleData.CurrentCraftId);

        if (SelectedCraftItem == null) {
            Debug.LogError("selectedCraftItem is no valid");
            SetCraftingItemByIndex(0);
        }

        SetCraftingFinishTime(craftingModuleData.SelectedCraft.CraftingFinishTime);
        TryCraftItem();
    }

    public void Tick()
    {
        if (!IsWorking) return;
        if (!TryCraftItem()) return;

        SetCrafted(true);
        TryStopWorking();
    }

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.OnClicked += OnBuildingClicked;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.OnClicked -= OnBuildingClicked;
    }

    protected override void OnWorkingStart()
    {
        base.OnWorkingStart();

        if (SelectedCraftItem != null && SelectedCraftItem.CraftingFinishTime != null) return;

        TryConsumeResources();
        ResetCraftingFinishTime();
    }

    protected override void OnWorkingStop()
    {
        base.OnWorkingStop();

        if (SelectedCraftItem == null) {
            Debug.Log($"CurrentCraftItem is not valid at {name}");
            return;
        }

        if (SelectedCraftItem.IsCraftingTimeFinished()) {
            RemoveCraftingFinishTime();
        }
    }

    protected override bool ShouldStartWorking()
    {
        if (!base.ShouldStartWorking()) return false;
        if (!cityStorage) return false;
        if (SelectedCraftItem == null) return false;
        if (SelectedCraftItem.IsCrafted) return false;
        if (SelectedCraftItem.CraftingFinishTime != null) return true;
        if (!IsEnoughResources(SelectedCraftItem.Definition)) return false;

        return true;
    }

    protected override bool ShouldStopWorking()
    {
        if (base.ShouldStopWorking()) return true;

        if (SelectedCraftItem == null) {
            Debug.Log($"CurrentCraftItem is not valid at {name}");
            return true;
        }

        if (SelectedCraftItem.IsCraftingTimeFinished()) return true;

        return false;
    }

    public void SetCrafted(bool value)
    {
        if (SelectedCraftItem == null) return;

        SelectedCraftItem.SetCrafted(value);
    }

    public void SetCraftingItem(CraftItemInstance craftItem)
    {
        if (craftItem == null) {
            Debug.LogError("craftItem is not valid");
            return;
        }

        if (!CraftItems.Contains(craftItem)) {
            Debug.LogError("Crafting module does not contain craft item");
            return;
        }

        if (SelectedCraftItem != null) {
            SelectedCraftItem.SetCraftSelected(false);
        }

        SelectedCraftItem = craftItem;
        SelectedCraftItem.SetCraftSelected(true);
    }

    public void SetCraftingItemByIndex(int index)
    {
        var levelData = ProductionLevelData;
        if (!levelData) {
            Debug.LogError("ProductionLevelData is null!", this);
            return;
        }

        var craftItemsLength = CraftItems.Count;
        if (index < 0 || index >= CraftItems.Count) {
            Debug.LogError($"Index {index} is out of range for CraftItems (Count: {CraftItems.Count})");
            return;
        }

        var craftItem = CraftItems[index];
        if (craftItem == null) {
            Debug.LogError($"craftItem is not valid by index {index}!");
            return;
        }

        SetCraftingItem(craftItem);
    }

    public void RemoveCraftignItem()
    {
        if (SelectedCraftItem != null) {
            SelectedCraftItem.SetCraftSelected(false);
        }

        SelectedCraftItem = null;
    }

    public void ResetCraftingFinishTime()
    {
        if (SelectedCraftItem == null) return;

        SelectedCraftItem.ResetCraftingFinishTime();
    }

    public void RemoveCraftingFinishTime()
    {
        if (SelectedCraftItem == null) return;

        SelectedCraftItem.SetCraftingFinishTime(null);
    }

    public void SetCraftingFinishTime(long? time)
    {
        if (SelectedCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return;
        }

        SelectedCraftItem.SetCraftingFinishTime(time);

        OnItemCraftStarted?.Invoke(SelectedCraftItem);
        OnModuleItemCraftStarted?.Invoke(this, SelectedCraftItem);
    }

    public void SetCraftingSpeedBonus(float value)
    {
        CraftingSpeedBonus = value;

        foreach (var item in CraftItems) {
            item.SetCraftingSpeedBonus(value);
        }
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

        var craftItemDefinition = SelectedCraftItem.Definition;
        if (!craftItemDefinition) {
            Debug.LogError($"CraftItemDefinition is not valid at {name}");
            return items;
        }

        var cityStorage = CityStorage.Instance;
        if (!cityStorage) return items;

        foreach (var consumeItem in craftItemDefinition.ConsumeResources) {
            var cityItem = cityStorage.Inventory.GetItemById(consumeItem.Definition.ItemId);
            var cityAmount = cityItem.Amount;

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
        if (!OwnedBuilding.BuildingData.IsRaidable) return false;
        //if (!IsWorking) return false;

        return true;
    }

    private void OnBuildingClicked()
    {
        if (!TryCollectItem()) return;

        SetCrafted(false);

        if (!TryStartWorking()) return;

        ResetCraftingFinishTime();
    }

    private void RegisterModule()
    {
        var manager = CraftingModulesManager.Instance;
        if (!manager) {
            Debug.Log("CraftingModulesManager not found on scene!");
            return;
        }

        manager.RegisterCraftingModule(this);
    }

    private void UnregisterModule()
    {
        var manager = CraftingModulesManager.Instance;
        if (!manager) return;

        manager.UnregisterCraftingModule(this);
    }

    private void CollectItem()
    {
        if (!cityStorage) return;

        if (SelectedCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return;
        }

        var craftItem = SelectedCraftItem.Definition.ProduceItem;
        if (craftItem == null) {
            Debug.LogError("CraftItem is not valid");
            return;
        }

        var craftItemDefinition = craftItem.Definition;
        if (!craftItemDefinition) {
            Debug.LogError("CraftItemDefinition is not valid");
            return;
        }

        var id = craftItemDefinition.ItemId;
        var amount = craftItem.Amount;
        cityStorage.Inventory.AddItem(id, amount);

        OnItemCollected?.Invoke(SelectedCraftItem);
        OnModuleItemCollected?.Invoke(this, SelectedCraftItem);
    }

    public bool TryConsumeResources()
    {
        if (!ShouldConsumeResources()) return false;

        foreach (var resource in SelectedCraftItem.Definition.ConsumeResources) {
            var id = resource.Definition.ItemId;
            var amount = resource.Amount;
            CityStorage.Instance.Inventory.RemoveItem(id, amount);
        }

        return true;
    }

    public bool TryRefundResources()
    {
        if (!ShouldRefundResources()) return false;

        foreach (var resource in SelectedCraftItem.Definition.ConsumeResources) {
            var id = resource.Definition.ItemId;
            var amount = resource.Amount;
            cityStorage.Inventory.AddItem(id, amount);
        }

        return true;
    }

    public bool TryCollectItem()
    {
        if (ShouldCollectItem()) {
            CollectItem();
            return true;
        }
        else {
            OnItemNotCollected?.Invoke(SelectedCraftItem);
            OnModuleItemNotCollected?.Invoke(this, SelectedCraftItem);
            return false;
        }
    }

    private void CreateCraftItems()
    {
        CraftItems.Clear();
        foreach (var def in ProductionLevelData.CraftItems) {
            var craftItemData = CraftItemData.Default();
            var item = def.CreateInstance(craftItemData);

            if (item == null) {
                Debug.Log("Craft item is not valid");
                continue;
            }

            CraftItems.Add(item);
        }
    }

    private bool TryCraftItem()
    {
        if (SelectedCraftItem == null) {
            Debug.LogError($"CurrentCraftItem is not valid at {name}");
            return false;
        }

        if (!SelectedCraftItem.IsCraftingTimeFinished()) return false;

        SetCrafted(true);
        TryStopWorking();

        OnItemCraftEnded?.Invoke(SelectedCraftItem);
        OnModuleItemCraftEnded?.Invoke(this, SelectedCraftItem);

        return true;
    }

    private bool IsEnoughResources(CraftItemDefinition craftItemDefinition)
    {
        if (!cityStorage) return false;

        if (!craftItemDefinition) {
            Debug.LogError($"CraftDefinition is not valid");
            return false;
        }

        foreach (var resource in craftItemDefinition.ConsumeResources) {
            var id = resource.Definition.ItemId;
            var neededAmount = resource.Amount;
            var storageAmount = cityStorage.Inventory.GetItemById(id).Amount;

            if (neededAmount > storageAmount) return false;
        }

        return true;
    }

    private bool ShouldConsumeResources()
    {
        if (!cityStorage) return false;
        if (!IsWorking) return false;

        if (SelectedCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return false;
        }

        if (SelectedCraftItem.IsCraftingTimeFinished()) return false;
        if (!IsEnoughResources(SelectedCraftItem.Definition)) return false;

        return true;
    }

    private bool ShouldRefundResources()
    {
        if (!cityStorage) return false;
        if (SelectedCraftItem == null) return false;
        if (SelectedCraftItem.IsCrafted) return false;

        return true;
    }

    private bool ShouldCollectItem()
    {
        if (SelectedCraftItem == null || !SelectedCraftItem.IsCrafted) return false;

        var storageItem = cityStorage.Inventory.GetItemById(SelectedCraftItem.Definition.ProduceItem.Definition.ItemId);
        if (storageItem.Amount >= storageItem.Stack.Amount) return false;

        return true;
    }
}