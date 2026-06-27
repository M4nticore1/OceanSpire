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
    public CraftItemInstance CurrentCraftItem { get; private set; }

    public bool IsReadyToCollect => CurrentCraftItem != null && CurrentCraftItem.IsCraftingFinished();
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

    public void Init(CraftingModuleData craftingModuleData)
    {
        CreateCraftItems();

        if (craftingModuleData != null) {
            SetCraftingItemByIndex(craftingModuleData.CurrentCraftId);

            var currentCraftData = craftingModuleData.CurrentCraft;
            if (currentCraftData != null) {
                SetCraftingFinishTime(currentCraftData.CraftingFinishTime);

                if (CurrentCraftItem != null && CurrentCraftItem.IsCraftingFinished()) {
                    SetCraftingInProgress(false);
                }
                else {
                    SetCraftingInProgress(currentCraftData.CraftingInProgress);
                }
            }
        }
        else {
            SetCraftingItemByIndex(0);
        }

        TryCraftItem();
    }

    public void Tick()
    {
        if (!IsWorking) return;
        if (!TryCraftItem()) return;

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

        if (!TryStartCrafting()) return;

        TryConsumeResources();
    }

    protected override void OnWorkingStop()
    {
        base.OnWorkingStop();

        RemoveCraftingFinishTime();
    }

    protected override bool ShouldStartWorking()
    {
        if (!base.ShouldStartWorking()) return false;
        if (!cityStorage) return false;
        if (IsReadyToCollect) return false;
        if (!IsEnoughResources(CurrentCraftItem.Definition)) return false;

        return true;
    }

    protected override bool ShouldStopWorking()
    {
        if (base.ShouldStopWorking()) return true;

        if (CurrentCraftItem == null) {
            Debug.Log($"CurrentCraftItem is not valid at {name}");
            return true;
        }

        if (CurrentCraftItem.IsCraftingFinished()) return true;

        return false;
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

        if (CurrentCraftItem != null) {
            CurrentCraftItem.SetCraftSelected(false);
        }

        CurrentCraftItem = craftItem;
        CurrentCraftItem.SetCraftSelected(true);

        if (IsWorking) {
            ResetCraftingFinishTime();
        }
        else {
            RemoveCraftingFinishTime();
        }
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
        if (CurrentCraftItem != null) {
            CurrentCraftItem.SetCraftSelected(false);
        }

        CurrentCraftItem = null;
    }

    public void ResetCraftingFinishTime()
    {
        if (CurrentCraftItem == null) return;

        CurrentCraftItem.ResetCraftingFinishTime();
    }

    public void RemoveCraftingFinishTime()
    {
        if (CurrentCraftItem == null) return;

        CurrentCraftItem.RemoveCraftingFinishTime();
    }

    public void SetCraftingFinishTime(long? time)
    {
        if (CurrentCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return;
        }

        CurrentCraftItem.SetCraftingFinishTime(time);
    }

    public void SetCraftingInProgress(bool value)
    {
        if (CurrentCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return;
        }

        CurrentCraftItem.SetCraftingInProgress(value);
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
        if (CraftItems.Contains(CurrentCraftItem)) {
            return CraftItems.IndexOf(CurrentCraftItem);
        }
        else {
            return 0;
        }
    }

    public float GetElectricityConsumption()
    {
        return ElectricityConsumption;
    }

    public ItemInstance[] GetRaidLoot()
    {
        var craftItemDefinition = CurrentCraftItem.Definition;
        if (!craftItemDefinition) {
            Debug.LogError($"CraftItemDefinition is not valid at {name}");
            return null;
        }

        var items = new List<ItemInstance>();
        foreach (var item in craftItemDefinition.ConsumeResources) {
            items.Add(item);
        }

        return items.ToArray();
    }

    private void OnBuildingClicked()
    {
        if (!TryCollectItem()) return;

        bool work = TryStartWorking();
        SetCraftingInProgress(work);

        if (!SelectManager.Instance) return;

        var selectComponent = SelectManager.Instance.SelectedComponent;
        if (!selectComponent) return;

        //selectComponent.Click();
    }

    private void CollectItem()
    {
        if (!cityStorage) return;

        if (CurrentCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return;
        }

        var craftItem = CurrentCraftItem.Definition.ProduceItem;
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

        ResetCraftingFinishTime();
        AssignFlicking();
    }

    private void AssignFlicking()
    {
        if (IsReadyToCollect) {
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

        foreach (var resource in CurrentCraftItem.Definition.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int amount = resource.Amount;
            CityStorage.Instance.Inventory.RemoveItem(id, amount);
        }

        return true;
    }

    public bool TryRefundResources()
    {
        if (!ShouldRefundResources()) return false;

        foreach (var resource in CurrentCraftItem.Definition.ConsumeResources) {
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

        if (CurrentCraftItem == null) {
            Debug.LogError($"CurrentCraftItem is not valid at {name}");
            return false;
        }

        if (!CurrentCraftItem.IsCraftingFinished()) return false;

        TryStopWorking();
        AssignFlicking();

        OnItemCrafted?.Invoke(CurrentCraftItem);
        OnModuleItemCrafted?.Invoke(this, CurrentCraftItem);

        return true;
    }

    private bool IsEnoughResources(CraftItemDefinition craftItemDefinition)
    {
        if (!CityStorage.Instance) return false;

        if (!craftItemDefinition) {
            Debug.LogError($"CraftDefinition is not valid");
            return false;
        }

        foreach (var resource in craftItemDefinition.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int neededAmount = resource.Amount;
            int storageAmount = CityStorage.Instance.Inventory.GetItemById(id).Amount;

            if (neededAmount > storageAmount) return false;
        }

        return true;
    }

    private bool ShouldConsumeResources()
    {
        if (!CityStorage.Instance) return false;
        if (!IsWorking) return false;

        if (CurrentCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return false;
        }

        if (CurrentCraftItem.CraftingInProgress) return false;
        if (!IsEnoughResources(CurrentCraftItem.Definition)) return false;

        return true;
    }

    private bool ShouldRefundResources()
    {
        if (!cityStorage) return false;
        if (CurrentCraftItem == null) return false;
        if (IsReadyToCollect) return false;

        return true;
    }

    private bool ShouldStartCrafting()
    {
        if (!ShouldStartWorking()) return false;

        return true;
    }

    private bool ShouldCollectItem()
    {
        if (!IsReadyToCollect) return false;

        return true;
    }
}