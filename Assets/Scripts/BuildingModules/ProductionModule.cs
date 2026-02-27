using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[AddComponentMenu("BuildingComponents/ProductionBuilding")]
public class ProductionModule : BuildingModule
{
    public ProductionModuleLevelData[] ProductionLevelsData => levelsData.OfType<ProductionModuleLevelData>().ToArray();
    public ProductionModuleLevelData ProductionLevelData => ProductionLevelsData[LevelIndex];
    public ProduceResource produceItem => ProductionLevelData ? (ProductionLevelData.producedResources.Count > currentProducedItemIndex ? ProductionLevelData.producedResources[currentProducedItemIndex] : null) : null;

    protected bool isProducting = false;
    public ItemInstance producedItem { get; private set; } = null;
    public float currentProductionTime { get; private set; } = 0.0f;
    private bool IsStorageFull => producedItem.Amount >= produceItem.maxAmount;

    private const float storageReadyToCollectAlpha = 0.5f;
    public bool isReadyToCollect { get; private set; } = false;

    public int currentProducedItemIndex { get; private set; } = 0;
    private const float produceFrequency = 1.0f;
    private float lastProduceTime = 0.0f;

    public const float collectLootFlickingMultiplier = 0.35f;

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onPlayerClicked += OnPlayerClicked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.onPlayerClicked -= OnPlayerClicked;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (!CanProduce()) return;

        ProcessProduce();
    }

    public void SetProduceTime(float time)
    {
        currentProductionTime = time;
        lastProduceTime = Time.time;

        BuildingLevelData buildingLevelData = OwnedBuilding.ConstructionLevelsData[OwnedBuilding.LevelIndex];
        ProductionModuleLevelData productionBuildingLevelData = levelsData[OwnedBuilding.LevelIndex] as ProductionModuleLevelData;

        float maxProductionTime = produceItem.produceTime * produceItem.maxAmount;
        float alpha = currentProductionTime / maxProductionTime;
        int lootAmount = (int)math.lerp(0, produceItem.maxAmount, alpha);

        if (lootAmount > producedItem.Amount) {
            AddItemAmount(lootAmount);
        }
    }

    public void RemoveItemAmount(int amount)
    {
        producedItem.RemoveAmount(amount);
        OnAmountChanged();
    }

    private void AddItemAmount(int amount)
    {
        producedItem.SetAmount(amount);
        OnAmountChanged();
    }

    // Overrides
    protected override void OnInit()
    {
        if (produceItem is ProduceResource resource) {
            producedItem = new ItemInstance(resource.produceItem.ItemData);
        }
    }

    protected override void OnBuildingStartWorking()
    {
        if (isProducting) return;

        StartProducting();
    }

    protected override void OnBuildingStopWorking()
    {
        StopProducting();
    }

    protected override void OnEnterBuilding(EntityCityNavigator navigator)
    {

    }

    protected override void OnExitBuilding(EntityCityNavigator navigator)
    {

    }

    // Production
    private void StartProducting()
    {
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

    private void ProcessProduce()
    {
        if (Time.time < lastProduceTime + produceFrequency) return;

        AddProducedTime(produceFrequency);
    }

    private bool CanProduce()
    {
        if (!isProducting) return false;
        if (IsStorageFull) return false;
        if (produceItem == null) return false;
        if (OwnedBuilding.currentWorkers.Count == 0) return false;
        return true;
    }

    private void AddProducedTime(float time)
    {
        SetProduceTime(currentProductionTime + time);
    }

    private void OnAmountChanged()
    {
        // Producing Time
        float remainder = currentProductionTime % produceItem.produceTime;
        float time = producedItem.Amount * produceItem.produceTime + remainder;
        SetProduceTime(time);

        // Flicking
        float alpha = (float)producedItem.Amount / produceItem.maxAmount;

        if (producedItem.Amount > 0 && (float)producedItem.Amount / produceItem.maxAmount >= storageReadyToCollectAlpha) {
            if (isReadyToCollect) return;

            isReadyToCollect = true;
            float multiplier = alpha * collectLootFlickingMultiplier;
            SetFlickingMultiplier(multiplier);
        }
        else {
            if (!isReadyToCollect) return;

            isReadyToCollect = false;
            SetFlickingMultiplier(0);
        }
    }

    // Take Resources
    private bool CanTake()
    {
        return producedItem.Amount > 0 && produceItem.maxAmount / producedItem.Amount > storageReadyToCollectAlpha;
    }

    // Events
    private void OnPlayerClicked(GameObject clicked)
    {
        if (clicked != gameObject) return;
        if (!CanTake()) return;

        EventBus.InvokeClickedProductionModule(this);
    }
}
