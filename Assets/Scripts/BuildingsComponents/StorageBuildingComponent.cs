using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("BuildingComponents/StorageBuildingComponent")]
public class StorageBuildingComponent : BuildingComponent
{
    public StorageBuildingLevelData[] StorageLevelsData => levelsData.OfType<StorageBuildingLevelData>().ToArray();
    public StorageBuildingLevelData StorageLevelData => StorageLevelsData[LevelIndex];
    public Dictionary<int, ItemInstance> storedItems = new Dictionary<int, ItemInstance>();

    protected override void BuildComponent()
    {
        base.BuildComponent();

        for (int i = 0; i < StorageLevelData.storageItems.Length; i++)
        {
            int id = StorageLevelData.storageItems[i].ItemData.ItemId;
            if (!storedItems.ContainsKey(id))
                storedItems.Add(id, new ItemInstance(StorageLevelData.storageItems[i].ItemData, 0));
            else
                Debug.LogError(OwnedBuilding.BuildingData.BuildingName + $" has the same item by id {id}");
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
            return storedItems[itemId].AddAmount(amount, StorageLevelData.storageItems[itemId].Amount);
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
