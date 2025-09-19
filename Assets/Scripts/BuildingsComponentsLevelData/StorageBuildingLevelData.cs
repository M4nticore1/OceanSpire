using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct StorageItemEntry
{
    public ItemData itemdata;
    public int capacity;
}

[Serializable]
public struct StorageItemCategoryEntry
{
    public ItemCategory itemCategory;
    public int capacity;
}

[CreateAssetMenu(fileName = "StorageBuildingLevelData", menuName = "Scriptable Objects/StorageBuildingLevelData")]
public class StorageBuildingLevelData : ScriptableObject
{
    public List<StorageItemEntry> storageItems = new List<StorageItemEntry>();
    public List<StorageItemCategoryEntry> storageItemCategories = new List<StorageItemCategoryEntry>();
}
