using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[AddComponentMenu("BuildingComponents/StorageBuildingComponent")]
public class StorageBuildingComponent : BuildingComponent
{
    [HideInInspector] public StorageBuildingLevelData levelData = null;
    public Dictionary<int, int> storedItems = new Dictionary<int, int>();

    public override void Build()
    {
        base.Build();

        levelData = levelsData[ownedBuilding.levelIndex] as StorageBuildingLevelData;

        AddStorageCapacity(levelData);

        for (int i = 0; i < levelData.storageItems.Count; i++)
        {
            int key = (int)levelData.storageItems[i].itemData.itemId;
            int value = levelData.storageItems[i].amount;
            storedItems.Add(key, 0);
        }
    }

    public override void LevelUp()
    {
        base.LevelUp();

        levelData = levelsData[ownedBuilding.levelIndex] as StorageBuildingLevelData;

        AddStorageCapacity(levelData);
        SubtractStorageCapacity(levelsData[ownedBuilding.levelIndex - 1] as StorageBuildingLevelData);
    }

    private void AddStorageCapacity(StorageBuildingLevelData levelData)
    {
		cityManager.AddStorageCapacity(levelData);
    }

    private void SubtractStorageCapacity(StorageBuildingLevelData levelData)
    {
		cityManager.SubtractStorageCapacity(levelData);
    }

    public int GiveItemAmount(ItemEntry item)
    {
        int amountToGive = item.amount;
        int storedAmount = storedItems[(int)item.itemData.itemId];
        if (amountToGive > storedAmount)
            amountToGive = storedAmount;

        cityManager.SpendItemById((int)item.itemData.itemId, amountToGive);

        return amountToGive;
    }
}
