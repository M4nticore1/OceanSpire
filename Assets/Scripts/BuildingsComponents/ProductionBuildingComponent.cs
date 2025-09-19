using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("BuildingComponents/ProductionBuildingComponent")]
public class ProductionBuildingComponent : BuildingComponent
{
    [SerializeField] private List<ProductionBuildingLevelData> productionBuildingLevelsData = new List<ProductionBuildingLevelData>();

    ItemInstance producedItem = null;
    private float produceTime = 0.0f;
    bool isStorageFull = false;

    private float storageFillPercentToReadyToCollect = 0.5f;
    [HideInInspector] public bool readyToCollect = false;

    [SerializeField] private CollectResourceWidget collectResourceWidgetPrefab = null;
    private CollectResourceWidget collectResourceWidget = null;

    private void Start()
    {
        ProductionBuildingLevelData levelsData = productionBuildingLevelsData[ownedBuilding.levelIndex];
        producedItem = new ItemInstance(levelsData.produceResource, 0, levelsData.maxResourceAmount);
    }

    private void Update()
    {
        Production();
    }

    private void Production()
    {
        int levelIndex = ownedBuilding.levelIndex;
        BuildingLevelData buildingLevelData = ownedBuilding.buildingLevelsData[levelIndex];
        ProductionBuildingLevelData productionBuildingLevelData = productionBuildingLevelsData[levelIndex];

        int currentPeopleCount = ownedBuilding.workersCount;
        int maxPeopleCount = buildingLevelData.maxResidentsCount;
        float productionTime = productionBuildingLevelData.produceTime;

        float productionSpeed = productionTime * (currentPeopleCount / maxPeopleCount);

        if (produceTime < productionBuildingLevelsData[ownedBuilding.levelIndex].produceTime && !isStorageFull)
        {
            produceTime += Time.deltaTime * productionSpeed;
        }
        else
        {
            if (!isStorageFull)
            {
                AddProduceResourceAmount();
            }
        }
    }

    private void AddProduceResourceAmount()
    {
        producedItem.AddAmount(productionBuildingLevelsData[ownedBuilding.levelIndex].produceResourceAmount);
        SetReadyToCollect();

        if (producedItem.amount == producedItem.maxAmount)
            isStorageFull = true;

        produceTime = 0.0f;
    }

    private void SetReadyToCollect()
    {
        if (producedItem.amount > 0 && producedItem.maxAmount / producedItem.amount >= storageFillPercentToReadyToCollect)
        {
            if (!readyToCollect)
            {
                readyToCollect = true;

                Vector3 position = new Vector3(0, 2, 2);
                Quaternion rotation = Quaternion.identity;

                if (collectResourceWidgetPrefab)
                {
                    collectResourceWidget = Instantiate(collectResourceWidgetPrefab, transform);
                    collectResourceWidget.gameObject.transform.SetLocalPositionAndRotation(position, rotation);
                }
            }
        }
        else
        {
            readyToCollect = false;

            if (collectResourceWidget)
            {
                Destroy(collectResourceWidget.gameObject);
            }
        }
    }

    public ItemInstance GetProducedItem()
    {
        return producedItem;
    }

    public ItemInstance TakeProducedItem(int remainingStorageCapacity)
    {
        if (producedItem.amount > 0)
        {
            isStorageFull = false;

            int amountToTake = 0;

            if (remainingStorageCapacity >= producedItem.amount)
                amountToTake = producedItem.amount;
            else
                amountToTake = producedItem.amount - remainingStorageCapacity;

            ItemInstance itemToTake = new ItemInstance(producedItem.itemData, producedItem.amount, producedItem.maxAmount);

            producedItem.SubtractAmount(amountToTake);
            SetReadyToCollect();

            return itemToTake;
        }
        else
        {
            return producedItem;
        }
    }
}
