using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StorageBuildingLevelData", menuName = "Scriptable Objects/StorageBuildingLevelData")]
public class StorageBuildingLevelData : BuildingComponentLevelData
{
    public List<ItemInstance> storageItems = new List<ItemInstance>();
    public List<ItemCategoryEntry> storageItemCategories = new List<ItemCategoryEntry>();

    public Dictionary<int, ItemInstance> storageItemsDict = new Dictionary<int, ItemInstance>();
    public Dictionary<int, ItemCategoryEntry> storageItemCategoriesDict = new Dictionary<int, ItemCategoryEntry>();

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < storageItems.Count; i++)
        {
            int id = storageItems[i].ItemData.ItemId;
            if (!storageItemsDict.ContainsKey(id))
                storageItemsDict.Add(id, storageItems[i]);
        }

        for (int i = 0; i < storageItemCategories.Count; i++)
        {
            int id = storageItems[i].ItemData.ItemId;
            if (!storageItemCategoriesDict.ContainsKey(id))
                storageItemCategoriesDict.Add(id, new ItemCategoryEntry(storageItems[i].ItemData.ItemCategory));
        }
    }
}
