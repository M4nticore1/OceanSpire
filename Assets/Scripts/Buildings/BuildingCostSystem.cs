using UnityEngine;

public class BuildingCostSystem : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private BuildingsLoader buildingsLoader;

    private void OnEnable()
    {
        Building.OnBuildingInited += HandleBuildingInited;
        Building.OnBuildingDemolished += HandleBuildingDemolished;
        UpgradeComponent.OnGlobalUpgradeStarted += HandleUpgradeStarted;
    }

    private void OnDisable()
    {
        Building.OnBuildingInited -= HandleBuildingInited;
        Building.OnBuildingDemolished -= HandleBuildingDemolished;
        UpgradeComponent.OnGlobalUpgradeStarted -= HandleUpgradeStarted;
    }

    private void HandleBuildingInited(Building building)
    {
        if (!ShouldWork()) return;

        SpendResources(building);
    }

    private void HandleUpgradeStarted(UpgradeComponent component)
    {
        if (!ShouldWork()) return;

        var building = component.GetComponent<Building>();
        if (!building) return;

        SpendResources(building);
    }

    private void HandleBuildingDemolished(Building building)
    {
        if (!ShouldWork()) return;

        RefundResources(building);
    }

    private void SpendResources(Building building)
    {
        foreach (var resource in building.GetResourcesToBuild()) {
            cityStorage.Inventory.RemoveItem(resource.Definition.ItemId, resource.Amount);
        }
    }

    private void RefundResources(Building building)
    {
        foreach (var resource in building.GetResourcesToRefund()) {
            cityStorage.Inventory.AddItem(resource.Definition.ItemId, resource.Amount);
        }
    }

    private bool ShouldWork()
    {
        return buildingsLoader.IsLoaded;
    }
}