using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("BuildingComponents/Storage Building Component")]
public class StorageBuildingModule : BuildingModule
{
    public StorageModuleLevelData[] StorageLevelsData => levelsData.OfType<StorageModuleLevelData>().ToArray();
    public StorageModuleLevelData StorageLevelData => StorageLevelsData[LevelIndex];
    public Dictionary<int, ItemInstance> storedItems = new Dictionary<int, ItemInstance>();

    protected override void OnInit()
    {
        for (int i = 0; i < StorageLevelData.storageItems.Length; i++)
        {
            int id = StorageLevelData.storageItems[i].ItemData.ItemId;
            if (!storedItems.ContainsKey(id))
                storedItems.Add(id, new ItemInstance(StorageLevelData.storageItems[i].ItemData, 0));
            else
                Debug.LogError(OwnedBuilding.BuildingData.BuildingName + $" has the same item by id {id}");
        }
    }

    protected override void OnBuildingStartWorking()
    {

    }

    protected override void OnBuildingStopWorking()
    {

    }

    protected override void OnEnterBuilding()
    {

    }

    protected override void OnExitBuilding()
    {

    }

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
        int amountToSpend = storedItems[itemId].RemoveAmount(amount);
        return amountToSpend;
    }
}
