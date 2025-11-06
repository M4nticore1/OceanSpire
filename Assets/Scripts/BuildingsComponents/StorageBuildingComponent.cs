using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[AddComponentMenu("BuildingComponents/StorageBuildingComponent")]
public class StorageBuildingComponent : BuildingComponent
{
    [HideInInspector] public StorageBuildingLevelData levelData = null;
    public Dictionary<int, ItemInstance> storedItems = new Dictionary<int, ItemInstance>();

    public override void Build()
    {
        base.Build();

        levelData = levelsData[ownedBuilding.levelIndex] as StorageBuildingLevelData;

        //AddStorageCapacity(levelData);

        for (int i = 0; i < levelData.storageItems.Count; i++)
        {
            int id = levelData.storageItems[i].ItemData.ItemId;
            storedItems.Add(id, levelData.storageItems[i]);
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

    public int AddItem(int itemId, int amount)
    {
        return AddItem_Internal(itemId, amount);
    }

    public int AddItem(ItemInstance item)
    {
        return AddItem_Internal(item.ItemData.ItemId, item.Amount);
    }

    private int AddItem_Internal(int itemId, int amount)
    {
        cityManager.AddItem(itemId, amount);
        return storedItems[itemId].AddAmount(amount);
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
        int storedAmount = storedItems[itemId].Amount;
        amountToGive = math.clamp(amountToGive, 0, storedAmount);

        cityManager.SpendItem(itemId, amountToGive);
        return amountToGive;
    }
}
