using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("BuildingComponents/StorageBuildingComponent")]
public class StorageBuildingComponent : BuildingComponent
{
    [HideInInspector] public StorageBuildingLevelData levelData = null;
    public Dictionary<int, ItemInstance> storedItems = new Dictionary<int, ItemInstance>();

    public override void Build(int newLevel)
    {
        base.Build(newLevel);

        Debug.Log("Build " + ownedBuilding.BuildingData.BuildingName);

        levelData = levelsData[newLevel] as StorageBuildingLevelData;

        //AddStorageCapacity(levelData);

        for (int i = 0; i < levelData.storageItems.Count; i++)
        {
            int id = levelData.storageItems[i].ItemData.ItemId;
            if (!storedItems.ContainsKey(id))
                storedItems.Add(id, new ItemInstance(levelData.storageItems[i].ItemData, 0));
            else
                Debug.LogError(ownedBuilding.BuildingData.BuildingName + $" has the same item by id {id}");
        }
    }

    //public override void UpdateLevel(int newLevel)
    //{
    //    base.UpdateLevel(newLevel);

    //    levelData = levelsData[ownedBuilding.levelIndex] as StorageBuildingLevelData;

    //    //AddStorageCapacity(levelData);
    //    //SubtractStorageCapacity(levelsData[ownedBuilding.levelIndex - 1] as StorageBuildingLevelData);
    //}

    //private void Update()
    //{

    //}

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
        Debug.Log("ADD");
        if (storedItems.ContainsKey(itemId))
            return storedItems[itemId].AddAmount(amount, levelData.storageItems[itemId].Amount);
        else
            return 0;
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
        int amountToSpend = storedItems[itemId].SubtractAmount(amount);
        return amountToSpend;
    }
}
