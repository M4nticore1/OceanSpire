using System.Linq;
using UnityEngine;

[AddComponentMenu("Building Modules/Production Building")]
public class ProductionModule : BuildingModule, ICurrentWorkersListener, IClickable, IElectricible, IRaidable
{
    [SerializeField] private SelectComponent selectComponent;

    public ProductionModuleLevelData[] ProductionLevelsData => levelsData.OfType<ProductionModuleLevelData>().ToArray();
    public ProductionModuleLevelData ProductionLevelData => ProductionLevelsData[OwnedBuilding.LevelComponent.level - 1];

    public ProducedItem currentProductingItem { get; private set; }
    public int currentProductingItemIndex { get; private set; }
    public float currentProductionTime { get; private set; }

    public bool isProducting { get; private set; } = false;
    public bool isReadyToCollect { get; private set; } = false;

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    private void Update()
    {
        if (!isWorking) return;

        ProcessProduce();
    }

    public void SetProduceTime(float time)
    {
        currentProductionTime = time;
        TryProduceItem();
    }

    public void SetProducedItemIndex(int index)
    {
        currentProductingItemIndex = index;
        SetProducedItemByIndex(currentProductingItemIndex);
    }

    // IClickable
    public void Click()
    {
        CollectItem();
    }

    public bool ShouldClick()
    {
        return isReadyToCollect;
    }

    // IElectricible
    public float GetElectricityConsumption()
    {
        return ElectricityConsumption;
    }

    public bool ShouldSpendElectricity()
    {
        return isWorking;
    }

    // IRaidable
    public ItemInstance GetRaidLoot()
    {
        return currentProductingItem.ConsumeResources[0];
    }

    // Workers
    public void OnCurrentWorkerAdded(InteractComponent interactor)
    {
        if (!ShouldStartWorking()) return;

        StartWorking();
    }

    public void OnCurrentWorkerRemoved(InteractComponent interactor)
    {
        if (ShouldStartWorking()) return;

        StopWorking();
    }

    protected override void Subscribe()
    {
        OwnedBuilding.onInited += OnInit;
        OwnedBuilding.onWorkStarted += OnWorkStarted;
    }

    protected override void Unsubscribe()
    {
        OwnedBuilding.onInited -= OnInit;
        OwnedBuilding.onWorkStarted -= OnWorkStarted;
    }

    private void OnInit()
    {
        SetProducedItemByIndex(currentProductingItemIndex);
    }

    private void OnWorkStarted()
    {
        StartProducting();
    }

    // Production
    private void TryCollectItem()
    {
        if (!isReadyToCollect) return;

        CollectItem();
    }

    private void CollectItem()
    {
        int id = currentProductingItem.ProductionItem.ItemData.ItemId;
        int amount = currentProductingItem.ProductionItem.Amount;
        CityStorage.Instance.Inventory.AddItemAmount(id, amount);

        isReadyToCollect = false;
        ResetProducedTime();

        if (ShouldStartWorking()) {
            StartWorking();
            StartProducting();
        }

        SetCollectable(false);
    }

    private void TryProduceItem()
    {
        if (currentProductionTime < currentProductingItem.ProduceTime) return;

        ProduceItem();
    }

    private void ProduceItem()
    {
        StopWorking();
        SetCollectable(true);
        isReadyToCollect = true;
        StopProducting();
    }

    private void StartProducting()
    {
        if (isProducting) return;

        ConsumeResources();
        isProducting = true;
    }

    private void StopProducting()
    {
        if (!isProducting) return;

        isProducting = false;
    }

    private void ConsumeResources()
    {
        foreach (var resource in currentProductingItem.ConsumeResources) {
            int id = resource.ItemData.ItemId;
            int amount = resource.Amount;
            CityStorage.Instance.Inventory.RemoveItemAmount(id, amount);
        }
    }

    private void RefundResources()
    {
        foreach (var resource in currentProductingItem.ConsumeResources) {
            int id = resource.ItemData.ItemId;
            int amount = resource.Amount;
            CityStorage.Instance.Inventory.AddItemAmount(id, amount);
        }
    }

    private void ProcessProduce()
    {
        AddProducedTime();
    }

    private void AddProducedTime()
    {
        SetProduceTime(currentProductionTime += Time.deltaTime);
    }

    private void ResetProducedTime()
    {
        SetProduceTime(0f);
    }

    private void SetCollectable(bool value)
    {
        isReadyToCollect = value;
        AssignFlicking();
    }

    private void AssignFlicking()
    {
        if (isReadyToCollect) {
            SetFlickingPower(1f);
        }
        else {
            SetFlickingPower(0);
        }
    }

    private void SetProducedItemByIndex(int index)
    {
        if (isProducting && !isReadyToCollect && currentProductingItem != null) {
            RefundResources();
            TryCollectItem();
            ResetProducedTime();
        }

        currentProductingItem = ProductionLevelData.producedResources[index];

        if (isProducting) {
            ConsumeResources();
        }
    }

    private bool ShouldStartWorking()
    {
        if (isReadyToCollect) return false;
        if (OwnedBuilding.WorkComponent.EnteredWorkers.Count == 0) return false;

        foreach (var resource in currentProductingItem.ConsumeResources) {
            int id = resource.ItemData.ItemId;
            int amount = resource.Amount;
            if (CityStorage.Instance.Inventory.itemsDict[id].item.Amount < amount) return false;
        }

        return true;
    }
}