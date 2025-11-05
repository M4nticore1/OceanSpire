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

        //AddStorageCapacity(levelData);

        for (int i = 0; i < levelData.storageItems.Count; i++)
        {
            int id = levelData.storageItems[i].ItemData.ItemId;
            int amount = levelData.storageItems[i].Amount;
            storedItems.Add(id, amount);
        }
    }

    public override void LevelUp()
    {
        base.LevelUp();

        levelData = levelsData[ownedBuilding.levelIndex] as StorageBuildingLevelData;

        //AddStorageCapacity(levelData);
        //SubtractStorageCapacity(levelsData[ownedBuilding.levelIndex - 1] as StorageBuildingLevelData);
    }

  //  private void AddStorageCapacity(StorageBuildingLevelData levelData)
  //  {
		//cityManager.AddStorageCapacity(levelData);
  //  }

  //  private void SubtractStorageCapacity(StorageBuildingLevelData levelData)
  //  {
		//cityManager.SubtractStorageCapacity(levelData);
  //  }

    public int StoreItem(int itemId, int amount)
    {
        return StoreItem_Internal(itemId, amount);
    }

    public int StoreItem(ItemInstance item)
    {
        return StoreItem_Internal((int)item.ItemData.ItemId, item.Amount);
    }

    private int StoreItem_Internal(int itemId, int amount)
    {
        int amountToGive = amount;
        int storedAmount = storedItems[itemId];
        if (amountToGive > storedAmount)
            amountToGive = storedAmount;

        cityManager.SpendItem(itemId, amountToGive);

        return amountToGive;
    }

    public int SpendItem(int itemId, int amount)
    {
        return SpendItem_Internal(itemId, amount);
    }

    public int SpendItem(ItemInstance item)
    {
        return SpendItem_Internal(item.ItemData.ItemId, item.Amount);
    }

    private int SpendItem_Internal(int itemId, int amount)
    {
        int amountToGive = amount;
        int storedAmount = storedItems[itemId];
        if (amountToGive > storedAmount)
            amountToGive = storedAmount;

        cityManager.SpendItem(itemId, amountToGive);
        Debug.Log(amountToGive);
        return amountToGive;
    }
}
