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

        for (int i = 0; i < BuildingsManager.Instance.BuiltFloors.Count; i++) {
            var floorModule = BuildingsManager.Instance.BuiltFloors[i];
            var floorBuilding = floorModule.GetComponent<Building>();

            foreach (var buildingPlace in floorModule.RoomBuildingPlaces) {
                var building = buildingPlace.PlacedBuilding;
                if (!building) continue;

                foreach (var electricible in building.GetComponents<IElectricible>()) {
                    if (electricible.ShouldSpendElectricity()) {
                        currentElectricityToDrain += electricible.GetElectricityConsumption();
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