using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ItemEntry
{
    public ItemData itemData;
    public int amount;
}

[Serializable]
public struct ItemCategoryEntry
{
    public ItemCategory itemCategory;
    public int capacity;
}

[CreateAssetMenu(fileName = "StorageBuildingLevelData", menuName = "Scriptable Objects/StorageBuildingLevelData")]
public class StorageBuildingLevelData : BuildingComponentLevelData
{
    public List<ItemEntry> storageItems = new List<ItemEntry>();
    public List<ItemCategoryEntry> storageItemCategories = new List<ItemCategoryEntry>();
}
