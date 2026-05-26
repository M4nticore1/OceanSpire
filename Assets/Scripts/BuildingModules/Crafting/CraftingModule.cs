using System.Linq;
using UnityEngine;

[AddComponentMenu("Building Modules/Crafting Module")]
public class CraftingModule : BuildingModule, IElectricible, IRaidable
{
    public ProductionModuleLevelData[] ProductionLevelsData => levelsData.OfType<ProductionModuleLevelData>().ToArray();
    public ProductionModuleLevelData ProductionLevelData => ProductionLevelsData[OwnedBuilding.LevelComponent.Level - 1];

    public CraftItem CurrentCraftItem { get; private set; }
    public int CurrentProductingItemIndex { get; private set; }
    public float CurrentProductionTime { get; private set; }

    public bool IsReadyToCollect { get; private set; } = false;

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    public void Init(CraftingModuleData craftingModuleData)
    {
        if (craftingModuleData != null) {
            SetProducedItemByIndex(craftingModuleData.CraftId);
            SetProduceTime(craftingModuleData.CraftingTime);
        }
        else {
            SetProducedItemByIndex(0);
        }
    }

    private void Update()
    {
        if (!IsWorking) return;

        ProcessProduce();
    }

    public void SetProducedItemByIndex(int index)
    {
        if (IsWorking && !IsReadyToCollect && CurrentCraftItem) {
            RefundResources();
            TryCollectItem();
            ResetProducedTime();
        }

        CurrentProductingItemIndex = index;
        CurrentCraftItem = ProductionLevelData.craftItems[index];

        if (IsWorking) {
            ConsumeResources();
        }
    }

    public void SetProduceTime(float time)
    {
        CurrentProductionTime = time;
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
        return CurrentCraftItem.ConsumeResources[0];
    }

    // Workers
    private void OnCurrentWorkerAdded(InteractComponent interactor)
    {
        if (!ShouldStartWorking()) return;

        StartWorking();
    }

    private void OnCurrentWorkerRemoved(InteractComponent interactor)
    {
        if (ShouldStartWorking()) return;

        StopWorking();
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

    // Production
    private bool TryCollectItem()
    {
        if (!IsReadyToCollect) return false;

        CollectItem();
        return true;
    }

    private void CollectItem()
    {
        int id = CurrentCraftItem.ProduceItem.Definition.ItemId;
        int amount = CurrentCraftItem.ProduceItem.Amount;
        CityStorage.Instance.Inventory.AddItem(id, amount);

        IsReadyToCollect = false;
        ResetProducedTime();

        if (ShouldStartWorking()) {
            StartWorking();
        }

        SetCollectable(false);
    }

    private void TryProduceItem()
    {
        if (CurrentProductionTime < CurrentCraftItem.ProduceTime) return;

        ProduceItem();
    }

    private void ProduceItem()
    {
        StopWorking();
        SetCollectable(true);
        IsReadyToCollect = true;
    }

    private void ConsumeResources()
    {
        foreach (var resource in CurrentCraftItem.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int amount = resource.Amount;
            CityStorage.Instance.Inventory.RemoveItem(id, amount);
        }
    }

    private void RefundResources()
    {
        foreach (var resource in CurrentCraftItem.ConsumeResources) {
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
        SetProduceTime(CurrentProductionTime += Time.deltaTime);
    }

    private void ResetProducedTime()
    {
        SetProduceTime(0f);
    }

    private void SetCollectable(bool value)
    {
        IsReadyToCollect = value;
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

        if (OwnedBuilding.WorkComponent.EnteredWorkers.Count == 0) return false;

        foreach (var resource in CurrentCraftItem.ConsumeResources) {
            int id = resource.Definition.ItemId;
            int amount = resource.Amount;

            if (CityStorage.Instance.Inventory.GetItemById(id).Amount < amount) return false;
        }

        return true;
    }
}