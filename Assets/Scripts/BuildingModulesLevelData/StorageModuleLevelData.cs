using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StorageBuildingLevelData", menuName = "Scriptable Objects/StorageBuildingLevelData")]
public class StorageModuleLevelData : BuildingModuleLevelData
{
    public ItemInstance[] storageItems;
    public ItemCategoryData[] storageItemCategories;
}