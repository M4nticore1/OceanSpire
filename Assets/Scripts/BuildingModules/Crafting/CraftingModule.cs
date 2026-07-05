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

    public event Action<CraftItemInstance> OnItemCrafted;

    public static event Action<CraftingModule, CraftItemInstance> OnModuleItemCrafted;

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
        Init(CraftingModuleData.Default());
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
            return;
        }

        SetCraftingFinishTime(craftingModuleData.SelectedCraft.CraftingFinishTime);
        SetCraftingInProgress(craftingModuleData.SelectedCraft.CraftingInProgress);

        if (TryCraftItem()) {
            SetCrafted(true);
            TryStopWorking();
            UpdateFlicking();
            return;
        }

        if (SelectedCraftItem.CraftingInProgress) {
            UpdateFlicking();
        }
    }

    public void Tick()
    {
        if (!IsWorking) return;
        if (!TryCraftItem()) return;

        SetCrafted(true);
        TryStopWorking();
        UpdateFlicking();
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

        if (SelectedCraftItem != null && SelectedCraftItem.CraftingInProgress) return;

        if (!TryStartCrafting()) return;

        TryConsumeResources();
    }

    protected override void OnWorkingStop()
    {
        base.OnWorkingStop();

        if (SelectedCraftItem != null && !SelectedCraftItem.IsCraftingFinished()) {
            RemoveCraftingFinishTime();
        }
    }

    protected override bool ShouldStartWorking()
    {
        if (!base.ShouldStartWorking()) return false;
        if (!cityStorage) return false;
        if (SelectedCraftItem == null) return false;
        if (SelectedCraftItem.IsCrafted) return false;
        if (SelectedCraftItem.CraftingInProgress) return true;

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

        if (SelectedCraftItem.IsCraftingFinished()) return true;

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
    }

    public void SetCraftingInProgress(bool value)
    {
        if (SelectedCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return;
        }

        SelectedCraftItem.SetCraftingInProgress(value);
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

        //if (SelectedCraftItem.IsCrafted) {
        //    items.Add(craftItemDefinition.ProduceItem);
        //    SetCrafted(false);
        //    UpdateFlicking();
        //}

        return items;
    }

    public bool CanBeRaided()
    {
        if (!OwnedBuilding.BuildingData.IsRaidable) return false;
        if (!IsWorking) return false;

        return true;
    }

    private void OnBuildingClicked()
    {
        if (!TryCollectItem()) return;

        SetCrafted(false);
        ResetCraftingFinishTime();
        UpdateFlicking();

        bool work = TryStartWorking();
        SetCraftingInProgress(work);

        if (!SelectManager.Instance) return;

        var selectComponent = SelectManager.Instance.SelectedComponent;
        if (!selectComponent) return;
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

        int id = craftItemDefinition.ItemId;
        int amount = craftItem.Amount;
        cityStorage.Inventory.AddItem(id, amount);
    }

    private void UpdateFlicking()
    {
        if (SelectedCraftItem != null && SelectedCraftItem.IsCrafted) {
            SetFlickingPower(1f);
        }
        else {
            SetFlickingPower(0);
        }
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

    public bool TryConsumeResources()
    {
        if (!ShouldConsumeResources()) return false;

        foreach (var resource in SelectedCraftItem.Definition.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int amount = resource.Amount;
            CityStorage.Instance.Inventory.RemoveItem(id, amount);
        }

        return true;
    }

    public bool TryRefundResources()
    {
        if (!ShouldRefundResources()) return false;

        foreach (var resource in SelectedCraftItem.Definition.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int amount = resource.Amount;
            cityStorage.Inventory.AddItem(id, amount);
        }

        return true;
    }

    public bool TryCollectItem()
    {
        if (!ShouldCollectItem()) return false;

        CollectItem();
        return true;
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

    private bool TryStartCrafting()
    {
        if (!ShouldStartCrafting()) return false;

        SetCraftingInProgress(true);
        ResetCraftingFinishTime();

        return true;
    }

    private bool TryCraftItem()
    {
        if (!IsWorking) return false;

        if (SelectedCraftItem == null) {
            Debug.LogError($"CurrentCraftItem is not valid at {name}");
            return false;
        }

        if (!SelectedCraftItem.IsCraftingFinished()) return false;

        OnItemCrafted?.Invoke(SelectedCraftItem);
        OnModuleItemCrafted?.Invoke(this, SelectedCraftItem);

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
            int id = resource.Definition.ItemId;
            int neededAmount = resource.Amount;
            int storageAmount = cityStorage.Inventory.GetItemById(id).Amount;

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

        if (SelectedCraftItem.CraftingInProgress) return false;
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

    private bool ShouldStartCrafting()
    {
        if (!ShouldStartWorking()) return false;

        return true;
    }

    private bool ShouldCollectItem()
    {
        if (SelectedCraftItem == null || !SelectedCraftItem.IsCrafted) return false;

        return true;
    }
}