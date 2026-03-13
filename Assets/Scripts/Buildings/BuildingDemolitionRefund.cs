using UnityEngine;

public class BuildingDemolitionRefund : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;

    private void OnEnable()
    {
        EventBus.onBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        EventBus.onBuildingDemolished -= OnBuildingDemolished;
    }

    private void OnBuildingDemolished(Building building)
    {
        foreach (var resource in building.GetDemolishionResources()) {
            cityStorage.Inventory.AddItemAmount(resource.ItemData.ItemId, resource.Amount);
        }
    }
}