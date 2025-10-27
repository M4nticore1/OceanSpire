using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[AddComponentMenu("BuildingComponents/StorageBuildingComponent")]
public class StorageBuildingComponent : BuildingComponent
{
    [HideInInspector] public StorageBuildingLevelData levelData = null;
    public List<ItemEntry> storedItems = new List<ItemEntry>();

    public override void Build()
    {
        base.Build();

        levelData = levelsData[ownedBuilding.levelIndex] as StorageBuildingLevelData;

        //Debug.Log("Storage: " + ownedBuilding.GetType());

        if (levelsData.Length > ownedBuilding.levelIndex && levelsData[ownedBuilding.levelIndex])
            AddStorageCapacity(levelData);
        else
            Debug.LogError("levelsData[ownedBuilding.levelIndex] is NULL");
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
        int storedAmount = storedItems[(int)item.itemData.itemId].amount;
        if (amountToGive > storedAmount)
            amountToGive = storedAmount;

        cityManager.SpendItemById((int)item.itemData.itemId, amountToGive);

        return amountToGive;
    }
}
