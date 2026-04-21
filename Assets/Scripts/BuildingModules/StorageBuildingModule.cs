using System.Linq;
using UnityEngine;

[AddComponentMenu("BuildingModules/Storage Building Module")]
public class StorageBuildingModule : BuildingModule
{
    public StorageModuleLevelData[] StorageLevelsData => levelsData.OfType<StorageModuleLevelData>().ToArray();
    public StorageModuleLevelData StorageLevelData => StorageLevelsData[OwnedBuilding.LevelComponent.level - 1];

    protected override void OnInit()
    {
        foreach (var category in StorageLevelData.storageItemCategories) {
            foreach (var itemDef in ItemsList.Instance.Items) {
                if (itemDef.ItemCategory != category.ItemCategory) continue;
                if (HasCategory(category.ItemCategory)) continue;

                int id = itemDef.ItemId;
                int amount = category.Amount;
                CityStorage.Instance.Inventory.AddItemMaxAmount(id, amount);
            }
        }

        foreach (ItemInstance item in StorageLevelData.storageItems) {
            int id = item.ItemData.ItemId;
            int amount = item.Amount;
            CityStorage.Instance.Inventory.AddItemMaxAmount(id, amount);
        }
    }

    protected override void OnDemolish()
    {
        foreach (ItemInstance item in StorageLevelData.storageItems) {
            int id = item.ItemData.ItemId;
            int amount = item.Amount;
            CityStorage.Instance.Inventory.RemoveItemMaxAmount(id, amount);
        }
    }

    protected override void OnBuildingStartWorking()
    {

    }

    protected override void OnBuildingStopWorking()
    {

    }

    private bool HasCategory(ItemCategory category)
    {
        foreach (var item in StorageLevelData.storageItems) {
            if (item.ItemData.ItemCategory == category) return true;
        }

        return false;
    }
}
