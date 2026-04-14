using UnityEngine;

public class BuildingCostSystem : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;

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
        foreach (var resource in building.GetResourcesToBuild()) {
            cityStorage.Inventory.RemoveItemAmount(resource.ItemData.ItemId, resource.Amount);
        }
    }

    private void OnBuildingDemolished(Building building)
    {
        foreach (var resource in building.GetResourcesToRefund()) {
            cityStorage.Inventory.AddItemAmount(resource.ItemData.ItemId, resource.Amount);
        }
    }
}