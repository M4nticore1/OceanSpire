using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StorageBuildingLevelData", menuName = "Scriptable Objects/StorageBuildingLevelData")]
public class StorageBuildingLevelData : BuildingModuleLevelData
{
    public ItemInstance[] storageItems;
    public ItemCategoryEntry[] storageItemCategories;

    public Dictionary<int, ItemInstance> storageItemsDict = new Dictionary<int, ItemInstance>();
    public Dictionary<int, ItemCategoryEntry> storageItemCategoriesDict = new Dictionary<int, ItemCategoryEntry>();

    private void Awake()
    {
        for (int i = 0; i < storageItems.Length; i++)
        {
            int id = storageItems[i].ItemData.ItemId;
            if (!storageItemsDict.ContainsKey(id))
                storageItemsDict.Add(id, storageItems[i]);
        }

        for (int i = 0; i < storageItemCategories.Length; i++)
        {
            int id = storageItems[i].ItemData.ItemId;
            if (!storageItemCategoriesDict.ContainsKey(id))
                storageItemCategoriesDict.Add(id, new ItemCategoryEntry(storageItems[i].ItemData.ItemCategory));
        }
    }
}
