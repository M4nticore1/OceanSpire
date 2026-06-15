using UnityEngine;

public class BuildingCostSystem : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private BuildingsLoader buildingsLoader;

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
            cityStorage.Inventory.RemoveItem(resource.Definition.ItemId, resource.Amount);
        }
    }

    private void OnBuildingDemolished(Building building)
    {
        if (!ShouldWork()) return;

        foreach (var resource in building.GetResourcesToRefund()) {
            cityStorage.Inventory.AddItem(resource.Definition.ItemId, resource.Amount);
        }
    }

    private bool ShouldWork()
    {
        return buildingsLoader.IsLoaded;
    }
}