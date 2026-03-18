using System.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[AddComponentMenu("BuildingComponents/ProductionBuilding")]
public class ProductionModule : BuildingModule, ICurrentWorkersListener, IClickable, IElectricible
{
    private CityStorage cityStorage;
    [SerializeField] private SelectComponent selectComponent;

    public ProductionModuleLevelData[] ProductionLevelsData => levelsData.OfType<ProductionModuleLevelData>().ToArray();
    public ProductionModuleLevelData ProductionLevelData => ProductionLevelsData[LevelIndex];
    public ProduceResource produceItem => ProductionLevelData ? (ProductionLevelData.producedResources.Length > currentProducedItemIndex ? ProductionLevelData.producedResources[currentProducedItemIndex] : null) : null;

    protected bool isProducting = false;
    public ItemInstance producedItem { get; private set; } = null;
    public float currentProductionTime { get; private set; } = 0.0f;
    private bool isBuildingStorageFull = false;

    public bool isReadyToCollect { get; private set; } = false;

    public int currentProducedItemIndex { get; private set; } = 0;

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    private void Start()
    {
        
    }

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

    public void CollectItem()
    {
        producedItem.SetAmount(0);

        int id = produceItem.produceItem.ItemData.ItemId;
        int amount = produceItem.produceItem.Amount;
        cityStorage.Inventory.AddItemAmount(id, amount);

        isBuildingStorageFull = false;
        ResetProducedTime();

        if (ShouldStartWorking()) {
            SetWorking(true);
            SetProducting(true);
        }

        SetCollectable(false);

        selectComponent.SetSelected(false);
    }

    private void TryProduceItem()
    {
        if (currentProductionTime < produceItem.produceTime) return;
        
        ProduceItem();
    }

    private void ProduceItem()
    {
        int amount = produceItem.produceItem.Amount;
        producedItem.SetAmount(amount);

        SetWorking(false);
        SetCollectable(true);
        isBuildingStorageFull = true;
        SetProducting(false);
    }

    // IClickable
    public void Click()
    {
        CollectItem();
    }

    public bool CanClick()
    {
        return isReadyToCollect;
    }

    // IElectricible
    public float GetElectricityConsumption()
    {
        return ElectricityConsumption;
    }

    public bool CanSpendElectricity()
    {
        return isWorking;
    }

    // Overrides
    protected override void OnInit()
    {
        cityStorage = FindAnyObjectByType<CityStorage>();

        if (produceItem is ProduceResource resource) {
            producedItem = new ItemInstance(resource.produceItem.ItemData);
        }
    }

    protected override void OnDemolish()
    {

    }

    protected override void OnBuildingStartWorking()
    {
        int id = producedItem.ItemData.ItemId;
        int amount = producedItem.Amount;
        cityStorage.Inventory.RemoveItemAmount(id, amount);

        SetProducting(true);
    }

    protected override void OnBuildingStopWorking()
    {

    }

    public void OnCurrentWorkerAdded(EntityInteractor interactor)
    {
        if (!ShouldStartWorking()) return;

        SetWorking(true);
    }

    public void OnCurrentWorkerRemoved(EntityInteractor interactor)
    {
        if (ShouldStartWorking()) return;

        SetWorking(false);
    }

    // Production
    private void SetProducting(bool value)
    {
        if (value == isProducting) return;

        isProducting = value;
        
        if (isProducting) {
            SpendConsumeResources();
        }
    }

    private void SpendConsumeResources()
    {
        foreach (var resource in produceItem.consumeResources) {
            int id = resource.ItemData.ItemId;
            int amount = resource.Amount;
            cityStorage.Inventory.RemoveItemAmount(id, amount);
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

    private bool ShouldStartWorking()
    {
        if (isBuildingStorageFull) return false;
        if (OwnedBuilding.currentWorkers.Count == 0) return false;

        foreach (var resource in produceItem.consumeResources) {
            int id = resource.ItemData.ItemId;
            int amount = resource.Amount;
            if (cityStorage.Inventory.itemsDict[id].item.Amount < amount) return false;
        }

        return true;
    }
}