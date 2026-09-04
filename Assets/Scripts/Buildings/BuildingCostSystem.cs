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

        if (building == null) {
            Debug.LogError($"[{nameof(BuildingCostSystem)}] Items To Upgrade is not valid!");
            return;
        }

        var level = building.LevelComponent.Level;
        SpendResources(building.GetResourcesToBuild(level));
    }

    private void HandleUpgradeStarted(UpgradeComponent component)
    {
        if (!ShouldWork()) return;

        if (component == null) {
            Debug.LogError($"[{nameof(BuildingCostSystem)}] Upgrade Component is not valid!");
            return;
        }

        var building = component.GetComponent<Building>();
        if (building == null) {
            Debug.LogError($"[{nameof(BuildingCostSystem)}] Building is not valid!");
            return;
        }

        SpendResources(building.GetResourcesToBuild(component.NextLevel));
    }

    private void HandleBuildingDemolished(Building building)
    {
        if (!ShouldWork()) return;

        if (building == null) {
            Debug.LogError($"[{nameof(BuildingCostSystem)}] Building is not valid!");
            return;
        }

        RefundResources(building.LevelDefinition.ResourcesToBuild);
    }

    private void SpendResources(ItemInstance[] itemsToSpend)
    {
        if (itemsToSpend == null) {
            Debug.LogError($"[{nameof(BuildingCostSystem)}] Items To Spend is not valid!");
            return;
        }

        foreach (var resource in itemsToSpend) {
            if (resource == null) {
                Debug.LogError($"[{nameof(BuildingCostSystem)}] Item is not valid!");
                continue;
            }

            cityStorage.Inventory.RemoveItemAmount(resource.Definition.ItemId, resource.Amount);
        }
    }

    private void RefundResources(ItemInstance[] itemsToUpgrade)
    {
        if (itemsToUpgrade == null) {
            Debug.LogError($"[{nameof(BuildingCostSystem)}] Items To Upgrade is not valid!");
            return;
        }

        foreach (var resource in itemsToUpgrade) {
            if (resource == null) {
                Debug.LogError($"[{nameof(BuildingCostSystem)}] Item is not valid!");
                continue;
            }

            cityStorage.Inventory.AddItemAmount(resource.Definition.ItemId, resource.Amount);
        }
    }

    private bool ShouldWork()
    {
        return buildingsLoader.IsLoaded;
    }
}