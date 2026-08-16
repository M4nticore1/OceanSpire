using UnityEngine;

public class ElectricityDrain : MonoBehaviour
{
    private float currentElectricityToDrain = 0f;

    private double lastCheckTime = 0;

    private void Update()
    {
        double nextTimeToCheck = lastCheckTime + 1d;
        if (Time.timeAsDouble < nextTimeToCheck) return;

        lastCheckTime = Time.timeAsDouble;

        var floors = BuildingsManager.Instance.BuiltFloors;
        for (int i = 0; i < floors.Count; i++) {
            var floorModule = floors[i];
            if (floorModule == null) continue;

            var floorBuilding = floorModule.OwnedBuilding;
            foreach (var buildingPlace in floorModule.RoomBuildingPlaces) {
                var placedBuilding = buildingPlace.PlacedBuilding;
                if (placedBuilding == null) continue;

                for (var j = 0; j < placedBuilding.buildingModules.Count; j++) {
                    var module = placedBuilding.buildingModules[j];
                    if (module == null) continue;

                    if (module.ShouldSpendElectricity()) {
                        currentElectricityToDrain += module.GetElectricityConsumption();
                    }
                }
            }
        }

        if (currentElectricityToDrain < 1) return;

        var amount = (int)currentElectricityToDrain;
        SpendElectricity(amount);
    }

    private void SpendElectricity(int amoount)
    {
        var id = ItemID.Electricity;
        CityStorage.Instance.Inventory.RemoveItemAmount(id, amoount);
        currentElectricityToDrain = 0f;
    }
}