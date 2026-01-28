using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[AddComponentMenu("BuildingComponents/ProductionBuilding")]
public class ProductionBuilding : BuildingComponent
{
    public ProductionBuildingLevelData[] ProductionLevelsData => levelsData.OfType<ProductionBuildingLevelData>().ToArray();
    public ProductionBuildingLevelData ProductionLevelData => ProductionLevelsData[LevelIndex];
    public ProducedResource producingItem => ProductionLevelData ? (ProductionLevelData.producedResources.Count > currentProducedItemIndex ? ProductionLevelData.producedResources[currentProducedItemIndex] : null) : null;

    protected bool isProducting = false;
    protected ItemInstance producedItem = null;
    public float currentProductionTime { get; private set; } = 0.0f;
    private bool IsStorageFull => producedItem.Amount == producingItem.maxAmount;

    private const float storageReadyToCollectAlpha = 0.5f;
    public bool isReadyToCollect { get; private set; } = false;

    public int currentProducedItemIndex { get; private set; } = 0;
    private const float produceFrequency = 1.0f;
    private float lastProduceTime = 0.0f;

    private void Update()
    {
        if (!OwnedBuilding.buildingPlace) return;

        if (isProducting)
            Production();
    }

    private void Start()
    {
        
    }

    // Overrides
    protected override void BuildComponent()
    {
        base.BuildComponent();

        if (producingItem is ProducedResource resource)
            producedItem = new ItemInstance(resource.producedResource.ItemData);
    }

    protected override void OnBuildingStartWorking()
    {
        base.OnBuildingStartWorking();
        StartProducting();
    }

    protected override void OnBuildingStopWorking()
    {
        base.OnBuildingStopWorking();
        StopProducting();
    }

    protected override void OnEnterBuilding()
    {
        base.OnEnterBuilding();
    }

    protected override void OnExitBuilding()
    {
        base.OnExitBuilding();
    }

    protected override void OnResidentStartWorking()
    {
        base.OnResidentStartWorking();
    }

    protected override void OnResidentStopWorking()
    {
        base.OnResidentStopWorking();
    }

    // Production
    private void StartProducting()
    {
        if (isProducting) return;

        isProducting = true;
        lastProduceTime = Time.time + produceFrequency;
        OnStartProducting();
    }

    private void StopProducting()
    {
        if (!isProducting) return;

        isProducting = false;
        OnStopProducting();
    }

    protected virtual void OnStartProducting()
    {

    }

    protected virtual void OnStopProducting()
    {
        Debug.Log("OnStopProduction");
    }

    private void Production()
    {
        if (!isProducting || OwnedBuilding.constructionComponent.isUnderConstruction || producingItem == null || OwnedBuilding.currentWorkers.Count == 0) return;

        if (Time.time > lastProduceTime + produceFrequency) {
            AddProducedTime(produceFrequency);
            lastProduceTime = Time.time;
        }
    }

    protected void OnLootProduct()
    {

    }

    public void SetProductionTime(float time)
    {
        currentProductionTime = time;

        ConstructionLevelData buildingLevelData = OwnedBuilding.ConstructionLevelsData[OwnedBuilding.LevelIndex];
        ProductionBuildingLevelData productionBuildingLevelData = levelsData[OwnedBuilding.LevelIndex] as ProductionBuildingLevelData;

        int currentPeopleCount = OwnedBuilding.currentWorkers.Count;
        int maxPeopleCount = buildingLevelData.maxResidentsCount;
        float maxProductionTime = producingItem.produceTime * producingItem.maxAmount;
        float productionSpeed = currentPeopleCount / maxPeopleCount;

        int lootAmount = (int)math.lerp(0, producingItem.maxAmount, currentProductionTime / maxProductionTime);
        if (lootAmount != producedItem.Amount) {
            SetProduceLootAmount(lootAmount);
            OnLootProduct();
        }
    }

    private void AddProducedTime(float time)
    {
        SetProductionTime(currentProductionTime + time);
    }

    private void SetProduceLootAmount(int amount)
    {
        producedItem.SetAmount(amount);
        float alpha = (float)producedItem.Amount / producingItem.maxAmount;

        if (producedItem.Amount > 0 && (float)producedItem.Amount / producingItem.maxAmount >= storageReadyToCollectAlpha) {
            if (isReadyToCollect) return;

            isReadyToCollect = true;
            float multiplier = alpha * CityManager.collectLootFlickingMultiplier;
            SetFlickingMultiplier(multiplier);
            Debug.Log(isReadyToCollect + " " + multiplier);
        }
        else {
            if (!isReadyToCollect) return;

            isReadyToCollect = false;
            SetFlickingMultiplier(0);
            Debug.Log("!isReadyToCollect");
        }
    }

    private void SubtractProducedLootAmount(int amount)
    {
        int newAmount = producedItem.Amount - amount;
        SetProduceLootAmount(newAmount);
    }

    public ItemInstance TakeProducedItem()
    {
        if (producedItem.Amount > 0) {
            ItemInstance storageItemInstance = CityManager.Instance.items[producedItem.ItemData.ItemId];
            int remainingStorageCapacity = producingItem.maxAmount - storageItemInstance.Amount;

            int amountToTake = 0;

            if (remainingStorageCapacity >= producedItem.Amount)
                amountToTake = producedItem.Amount;
            else
                amountToTake = producedItem.Amount - remainingStorageCapacity;

            SubtractProducedLootAmount(amountToTake);
            currentProductionTime -= amountToTake * producingItem.produceTime;
        }

        return producedItem;
    }
}
