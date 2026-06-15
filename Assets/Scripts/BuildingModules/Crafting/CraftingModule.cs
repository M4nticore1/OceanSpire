using System.Linq;
using UnityEngine;

[AddComponentMenu("Building Modules/Crafting Module")]
public class CraftingModule : BuildingModule, IElectricible, IRaidable
{
    public ProductionModuleLevelData[] ProductionLevelsData => levelsData.OfType<ProductionModuleLevelData>().ToArray();
    public ProductionModuleLevelData ProductionLevelData => ProductionLevelsData[OwnedBuilding.LevelComponent.Level - 1];

    public CraftItemDefinition CurrentCraftItemDefinition { get; private set; }
    public int CurrentProductingItemIndex { get; private set; }

    public CraftItemInstance CurrentCraftItem { get; private set; }
    public bool IsReadyToCollect => CurrentCraftItem != null ? CurrentCraftItem.IsReadyToCollect() : false;

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    public void Init(CraftingModuleData craftingModuleData)
    {
        if (craftingModuleData != null) {
            SetCraftingItemByIndex(craftingModuleData.CraftId);
            SetCraftingTime(craftingModuleData.CraftingTime);
        }
        else {
            SetCraftingItemByIndex(0);
        }
    }

    private void Update()
    {
        if (!IsWorking) return;

        ProcessProduce();
    }

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.OnClicked += OnBuildingClicked;
        OwnedBuilding.onCurrentWorkerAdded += OnCurrentWorkerAdded;
        OwnedBuilding.onCurrentWorkerRemoved += OnCurrentWorkerRemoved;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.OnClicked -= OnBuildingClicked;
        OwnedBuilding.onCurrentWorkerAdded -= OnCurrentWorkerAdded;
        OwnedBuilding.onCurrentWorkerRemoved -= OnCurrentWorkerRemoved;
    }

    public void SetCraftingItemByIndex(int index)
    {
        if (IsWorking && !IsReadyToCollect && CurrentCraftItemDefinition) {
            RefundResources();
            TryCollectItem();
            ResetProducedTime();
        }

        CurrentProductingItemIndex = index;
        CurrentCraftItemDefinition = ProductionLevelData.CraftItems[index];
        CurrentCraftItem = CurrentCraftItemDefinition.CreateInstance(CraftItemData.Default());

        if (IsWorking) {
            ConsumeResources();
        }
    }

    public void SetCraftingTime(float time)
    {
        if (CurrentCraftItem == null) {
            Debug.LogError("CurrentCraftItem is not valid");
            return;
        }

        CurrentCraftItem.SetCurrentCraftingTime(time);
        TryProduceItem();
    }

    // Click
    public void OnBuildingClicked()
    {
        if (!TryCollectItem()) return;

        SelectManager.Instance.SelectedComponent.Click();
    }

    // IElectricible
    public float GetElectricityConsumption()
    {
        return ElectricityConsumption;
    }

    public bool ShouldSpendElectricity()
    {
        return IsWorking;
    }

    // IRaidable
    public ItemInstance GetRaidLoot()
    {
        return CurrentCraftItemDefinition.ConsumeResources[0];
    }

    // Workers
    private void OnCurrentWorkerAdded(BuildingInteractComponent interactor)
    {
        if (!ShouldStartWorking()) return;

        StartWorking();
    }

    private void OnCurrentWorkerRemoved(BuildingInteractComponent interactor)
    {
        if (!ShouldStopWorking()) return;

        StopWorking();
    }

    // Production
    private bool TryCollectItem()
    {
        if (!IsReadyToCollect) return false;

        CollectItem();
        return true;
    }

    private void CollectItem()
    {
        int id = CurrentCraftItemDefinition.ProduceItem.Definition.ItemId;
        int amount = CurrentCraftItemDefinition.ProduceItem.Amount;
        CityStorage.Instance.Inventory.AddItem(id, amount);

        ResetProducedTime();

        if (ShouldStartWorking()) {
            StartWorking();
        }

        SetCollectable(false);
    }

    private void TryProduceItem()
    {
        if (IsReadyToCollect) return;

        ProduceItem();
    }

    private void ProduceItem()
    {
        StopWorking();
        SetCollectable(true);
    }

    private void ConsumeResources()
    {
        foreach (var resource in CurrentCraftItemDefinition.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int amount = resource.Amount;
            CityStorage.Instance.Inventory.RemoveItem(id, amount);
        }
    }

    private void RefundResources()
    {
        foreach (var resource in CurrentCraftItemDefinition.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int amount = resource.Amount;
            CityStorage.Instance.Inventory.AddItem(id, amount);
        }
    }

    private void ProcessProduce()
    {
        AddProducedTime();
    }

    private void AddProducedTime()
    {
        SetCraftingTime(CurrentCraftItem.CurrentCraftingTime + Time.deltaTime);
    }

    private void ResetProducedTime()
    {
        SetCraftingTime(0f);
    }

    private void SetCollectable(bool value)
    {
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

    private bool ShouldStartWorking()
    {
        if (IsReadyToCollect) return false;

        if (OwnedBuilding.WorkComponent.CurrentWorkers.Count == 0) return false;

        foreach (var resource in CurrentCraftItemDefinition.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int amount = resource.Amount;

            if (CityStorage.Instance.Inventory.GetItemById(id).Amount < amount) return false;
        }

        return true;
    }

    private bool ShouldStopWorking()
    {
        if (OwnedBuilding.WorkComponent.CurrentWorkers.Count > 0) return false;

        return true;
    }
}