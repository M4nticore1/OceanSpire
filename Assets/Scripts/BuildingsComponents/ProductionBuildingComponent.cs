using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("BuildingComponents/ProductionBuildingComponent")]
public class ProductionBuildingComponent : BuildingComponent
{
    [HideInInspector] public ProductionBuildingLevelData levelData = null;

    ItemInstance producedItem = null;
    private float produceTime = 0.0f;
    bool isStorageFull = false;

    private float storageFillPercentToReadyToCollect = 0.5f;
    [HideInInspector] public bool isReadyToCollect = false;

    [SerializeField] private CollectResourceWidget collectResourceWidgetPrefab = null;
    private CollectResourceWidget collectResourceWidget = null;

    private void Update()
    {
        Production();
    }

    public override void Build()
    {
        base.Build();

        levelData = levelsData[ownedBuilding.levelIndex] as ProductionBuildingLevelData;
        producedItem = new ItemInstance(levelData.produceResource);
    }

    public override void LevelUp()
    {
        base.LevelUp();

        levelData = levelsData[ownedBuilding.levelIndex] as ProductionBuildingLevelData;
    }

    private void Production()
    {
        if (ownedBuilding)
        {
            if (!ownedBuilding.isUnderConstruction)
            {
                BuildingLevelData buildingLevelData = ownedBuilding.buildingLevelsData[ownedBuilding.levelIndex];
                ProductionBuildingLevelData productionBuildingLevelData = levelsData[ownedBuilding.levelIndex] as ProductionBuildingLevelData;

                int currentPeopleCount = ownedBuilding.currentWorkers.Count;

                if (currentPeopleCount > 0)
                {
                    int maxPeopleCount = buildingLevelData.maxResidentsCount;
                    float productionTime = productionBuildingLevelData.produceTime;

                    float productionSpeed = productionTime * ((float)currentPeopleCount / (float)maxPeopleCount);

                    if (produceTime < productionBuildingLevelData.produceTime && !isStorageFull)
                    {
                        //Debug.Log("produceTime " + produceTime);
                        produceTime += Time.deltaTime * productionSpeed;
                    }
                    else
                    {
                        //Debug.Log("AddProduceResourceAmount");

                        if (!isStorageFull)
                        {
                            AddProduceResourceAmount();
                        }
                    }
                }
            }
        }
        else
            Debug.LogError("ownedBuilding is NULL");
    }

    private void AddProduceResourceAmount()
    {
        producedItem.AddAmount(levelData.produceResourceAmount);
        SetReadyToCollect();

        if (producedItem.Amount == levelData.maxResourceAmount)
            isStorageFull = true;

        produceTime = 0.0f;
    }

    private void SetReadyToCollect()
    {
        if (producedItem.Amount > 0 && levelData.maxResourceAmount / producedItem.Amount >= storageFillPercentToReadyToCollect)
        {
            if (!isReadyToCollect)
            {
                Debug.Log("readyToCollect");
                isReadyToCollect = true;

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
            isReadyToCollect = false;

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

    public ItemInstance TakeProducedItem()
    {
        if (producedItem.Amount > 0)
        {
            ItemInstance storageItemInstance = cityManager.items[producedItem.ItemData.ItemId];
            int remainingStorageCapacity = levelData.maxResourceAmount - storageItemInstance.Amount;

            isStorageFull = false;

            int amountToTake = 0;

            if (remainingStorageCapacity >= producedItem.Amount)
                amountToTake = producedItem.Amount;
            else
                amountToTake = producedItem.Amount - remainingStorageCapacity;

            ItemInstance itemToTake = new ItemInstance(producedItem.ItemData, amountToTake);
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
