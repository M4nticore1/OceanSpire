using System.Linq;
using UnityEngine;

[AddComponentMenu("Building Modules/Production Module")]
public class ProductionModule : BuildingModule, IElectricible, IRaidable
{
    public ProductionModuleLevelData[] ProductionLevelsData => levelsData.OfType<ProductionModuleLevelData>().ToArray();
    public ProductionModuleLevelData ProductionLevelData => ProductionLevelsData[OwnedBuilding.LevelComponent.Level - 1];

    public CraftItem CurrentCraftItem { get; private set; }
    public int CurrentProductingItemIndex { get; private set; }
    public float CurrentProductionTime { get; private set; }

    public bool IsReadyToCollect { get; private set; } = false;

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    private void Update()
    {
        if (!IsWorking) return;

        ProcessProduce();
    }

    public void SetProduceTime(float time)
    {
        CurrentProductionTime = time;
        TryProduceItem();
    }

    public void SetProducedItemIndex(int index)
    {
        CurrentProductingItemIndex = index;
        SetProducedItemByIndex(CurrentProductingItemIndex);
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

    // Click
    public void OnBuildingClicked()
    {
        if (!TryCollectItem()) return;

        SelectManager.Instance.SelectedComponent.Click();
    }

    protected override void Subscribe()
    {
        OwnedBuilding.onInited += OnInit;
        OwnedBuilding.onClicked += OnBuildingClicked;
        OwnedBuilding.onCurrentWorkerAdded += OnCurrentWorkerAdded;
        OwnedBuilding.onCurrentWorkerRemoved += OnCurrentWorkerRemoved;
    }

    protected override void Unsubscribe()
    {
        OwnedBuilding.onInited -= OnInit;
        OwnedBuilding.onClicked -= OnBuildingClicked;
        OwnedBuilding.onCurrentWorkerAdded -= OnCurrentWorkerAdded;
        OwnedBuilding.onCurrentWorkerRemoved -= OnCurrentWorkerRemoved;
    }

    private void OnInit()
    {
        SetProducedItemByIndex(CurrentProductingItemIndex);
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

    private void SetProducedItemByIndex(int index)
    {
        if (IsWorking && !IsReadyToCollect && CurrentCraftItem != null) {
            RefundResources();
            TryCollectItem();
            ResetProducedTime();
        }

        CurrentCraftItem = ProductionLevelData.craftItems[index];

        if (IsWorking) {
            ConsumeResources();
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