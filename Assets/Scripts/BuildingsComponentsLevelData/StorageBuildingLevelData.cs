using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StorageBuildingLevelData", menuName = "Scriptable Objects/StorageBuildingLevelData")]
public class StorageBuildingLevelData : BuildingComponentLevelData
{
    public List<ItemInstance> storageItems = new List<ItemInstance>();
    public List<ItemCategoryEntry> storageItemCategories = new List<ItemCategoryEntry>();
}
