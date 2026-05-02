using UnityEngine;

public class BuildingCostSystem : MonoBehaviour
{
    private void OnEnable()
    {
        Building.onBuildingInited += OnBuildingInited;
        Building.onBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        Building.onBuildingInited -= OnBuildingInited;
        Building.onBuildingDemolished -= OnBuildingDemolished;
    }

    private void OnBuildingInited(Building building)
    {
        if (!ShouldWork()) return;

        foreach (var resource in building.GetResourcesToBuild()) {
            CityStorage.Instance.Inventory.RemoveItemAmount(resource.ItemData.ItemId, resource.Amount);
        }
    }

    private void OnBuildingDemolished(Building building)
    {
        if (!ShouldWork()) return;

        foreach (var resource in building.GetResourcesToRefund()) {
            CityStorage.Instance.Inventory.AddItemAmount(resource.ItemData.ItemId, resource.Amount);
        }
    }

    private bool ShouldWork()
    {
        return BuildingsLoader.Instance.IsLoaded;
    }
}