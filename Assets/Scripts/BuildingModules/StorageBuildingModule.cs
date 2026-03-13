using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("BuildingModules/Storage Building Module")]
public class StorageBuildingModule : BuildingModule
{
    private CityStorage cityStorage;

    public StorageModuleLevelData[] StorageLevelsData => levelsData.OfType<StorageModuleLevelData>().ToArray();
    public StorageModuleLevelData StorageLevelData => StorageLevelsData[LevelIndex];

    protected override void OnInit()
    {
        cityStorage = FindAnyObjectByType<CityStorage>();

        foreach (ItemInstance item in StorageLevelData.storageItems) {
            int id = item.ItemData.ItemId;
            int amount = item.Amount;
            cityStorage.Inventory.AddItemMaxAmount(id, amount);
        }
    }

    protected override void OnDemolish()
    {
        foreach (ItemInstance item in StorageLevelData.storageItems) {
            int id = item.ItemData.ItemId;
            int amount = item.Amount;
            cityStorage.Inventory.RemoveItemMaxAmount(id, amount);
        }
    }

    protected override void OnBuildingStartWorking()
    {

    }

    protected override void OnBuildingStopWorking()
    {

    }

    protected override void OnEnterBuilding(EntityCityNavigator navigator)
    {

    }

    protected override void OnExitBuilding(EntityCityNavigator navigator)
    {

    }
}
