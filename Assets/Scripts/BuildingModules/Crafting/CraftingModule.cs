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

    public CraftItemDefinition CurrentCraftItemDefinition { get; private set; }
    public CraftItemInstance CurrentCraftItem { get; private set; }
    public int CurrentProductingItemIndex { get; private set; }

    public bool IsReadyToCollect => CurrentCraftItem != null && CurrentCraftItem.IsCraftingFinished();

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

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
        if (craftingModuleData != null) {
            SetCraftingItemByIndex(craftingModuleData.CurrentCraftId);

            if (craftingModuleData.CurrentCraft != null) {
                SetCraftingTime(craftingModuleData.CurrentCraft.CurrentCraftingTime);
                SetIsCrafting(craftingModuleData.CurrentCraft.CraftingInProgress);
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

        AddCraftingTime();
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

        TryConsumeResources();
        TryStartCrafting();
    }

    protected override bool ShouldStartWorking()
    {
        if (!base.ShouldStartWorking()) return false;
        if (!CityStorage.Instance) return false;
        if (IsReadyToCollect) return false;
        if (!IsEnoughResources(CurrentCraftItemDefinition)) return false;

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

    public void SetCraftingItemByIndex(int index)
    {
        var levelData = ProductionLevelData;
        if (levelData == null) {
            Debug.LogError("ProductionLevelData is null!", this);
            return;
        }

        if (levelData.CraftItems.Length <= index) {
            Debug.LogError("CraftItemIndex is over than CraftItems length!");
            return;
        }

        var definition = levelData.CraftItems[index];
        if (!definition) {
            Debug.LogError($"CraftItemDefinition is not valid by index {index}!");
            return;
        }

        var instance = definition.CreateInstance(CraftItemData.Default());
        if (instance == null) {
            Debug.LogError($"CraftItemInstance is not valid from {definition}!");
            return;
        }

        CurrentProductingItemIndex = index;
        CurrentCraftItemDefinition = definition;
        CurrentCraftItem = instance;
    }

    public void ResetProducedTime()
    {
        SetCraftingTime(0f);
    }

    public void SetCraftingTime(float time)
    {
        if (CurrentCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return;
        }

        CurrentCraftItem.SetCurrentCraftingTime(time);
    }

    public void SetIsCrafting(bool value)
    {
        if (CurrentCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return;
        }

        CurrentCraftItem.SetIsCrafting(value);
    }

    private void OnBuildingClicked()
    {
        if (!TryCollectItem()) return;

        bool work = TryStartWorking();
        SetIsCrafting(work);

        if (!SelectManager.Instance) return;

        var selectComponent = SelectManager.Instance.SelectedComponent;
        if (!selectComponent) return;

        selectComponent.Click();
    }

    public float GetElectricityConsumption()
    {
        return ElectricityConsumption;
    }

    public bool ShouldSpendElectricity()
    {
        return IsWorking;
    }

    public ItemInstance[] GetRaidLoot()
    {
        if (!CurrentCraftItemDefinition) {
            Debug.LogError($"CraftItemDefinition is not valid at {name}");
            return null;
        }

        var items = new List<ItemInstance>();
        foreach (var item in CurrentCraftItemDefinition.ConsumeResources) {
            items.Add(item);
        }

        return items.ToArray();
    }

    private void CollectItem()
    {
        if (!CityStorage.Instance) return;

        if (CurrentCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return;
        }

        var craftItem = CurrentCraftItemDefinition.ProduceItem;
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
        CityStorage.Instance.Inventory.AddItem(id, amount);

        ResetProducedTime();
        AssignFlicking();
    }

    private void AddCraftingTime()
    {
        SetCraftingTime(CurrentCraftItem.CurrentCraftingTime + Time.deltaTime);
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

        foreach (var resource in CurrentCraftItemDefinition.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int amount = resource.Amount;
            CityStorage.Instance.Inventory.RemoveItem(id, amount);
        }

        return true;
    }

    public bool TryRefundResources()
    {
        if (!ShouldRefundResources()) return false;

        foreach (var resource in CurrentCraftItemDefinition.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int amount = resource.Amount;
            CityStorage.Instance.Inventory.AddItem(id, amount);
        }

        return true;
    }

    public bool TryCollectItem()
    {
        if (!ShouldCollectItem()) return false;

        CollectItem();
        return true;
    }

    private bool TryStartCrafting()
    {
        if (!ShouldStartCrafting()) return false;

        SetIsCrafting(true);
        return true;
    }

    private bool TryCraftItem()
    {
        if (CurrentCraftItem == null) {
            Debug.LogError($"CurrentCraftItem is not valid at {name}");
            return false;
        }

        if (!CurrentCraftItem.IsCraftingFinished()) return false;

        TryStopWorking();
        AssignFlicking();

        return true;
    }

    private bool IsEnoughResources(CraftItemDefinition craft)
    {
        if (!CityStorage.Instance) return false;

        if (!craft) {
            Debug.LogError($"CraftDefinition is not valid");
            return false;
        }

        foreach (var resource in craft.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int amount = resource.Amount;

            if (amount > CityStorage.Instance.Inventory.GetItemById(id).Amount) return false;
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

        if (CurrentCraftItem.IsCrafting) return false;
        if (!IsEnoughResources(CurrentCraftItemDefinition)) return false;

        return true;
    }

    private bool ShouldRefundResources()
    {
        if (!CityStorage.Instance) return false;
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