using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ItemAmountEntry
{
    public ItemData itemdata;
    public int capacity;
}

[Serializable]
public struct ItemCategoryAmountEntry
{
    public ItemCategory itemCategory;
    public int capacity;
}

[CreateAssetMenu(fileName = "StorageBuildingLevelData", menuName = "Scriptable Objects/StorageBuildingLevelData")]
public class StorageBuildingLevelData : ScriptableObject
{
    public List<ItemAmountEntry> storageItems = new List<ItemAmountEntry>();
    public List<ItemCategoryAmountEntry> storageItemCategories = new List<ItemCategoryAmountEntry>();
}
