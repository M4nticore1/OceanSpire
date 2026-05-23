using UnityEngine;

public class BuildingCostSystem : MonoBehaviour
{
    private void OnEnable()
    {
        Building.OnBuildingInited += OnBuildingInited;
        Building.OnBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        Building.OnBuildingInited -= OnBuildingInited;
        Building.OnBuildingDemolished -= OnBuildingDemolished;
    }

    private void OnBuildingInited(Building building)
    {
        if (!ShouldWork()) return;

        foreach (var resource in building.GetResourcesToBuild()) {
            CityStorage.Instance.Inventory.RemoveItem(resource.Definition.ItemId, resource.Amount);
        }
    }

    private void OnBuildingDemolished(Building building)
    {
        if (!ShouldWork()) return;

        foreach (var resource in building.GetResourcesToRefund()) {
            CityStorage.Instance.Inventory.AddItem(resource.Definition.ItemId, resource.Amount);
        }
    }

    private bool ShouldWork()
    {
        return BuildingsLoader.Instance.IsLoaded;
    }
}