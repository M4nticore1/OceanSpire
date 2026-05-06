using UnityEngine;

[CreateAssetMenu(fileName = "StorageBuildingLevelData", menuName = "Scriptable Objects/StorageBuildingLevelData")]
public class StorageModuleLevelData : BuildingModuleLevelData
{
    [SerializeField] private ItemStack[] stacks;
    public ItemStack[] Stacks => stacks;

    public ItemInstance[] storageItems;
    public ItemCategoryData[] storageItemCategories;
}