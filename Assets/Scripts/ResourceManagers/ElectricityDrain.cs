using UnityEngine;

public class ElectricityDrain : MonoBehaviour
{
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private CityStorage cityStorage;

    private float currentElectricityToDrain = 0f;

    private double lastCheckTime = 0;

    private void Update()
    {
        double nextTimeToCheck = lastCheckTime + 1d;
        if (Time.timeAsDouble < nextTimeToCheck) return;

        lastCheckTime = Time.timeAsDouble;

        foreach (var floorModule in buildingsManager.BuiltFloors) {
            var floorBuilding = floorModule.GetComponent<Building>();

            foreach (var buildingPlace in floorModule.RoomBuildingPlaces) {
                var building = buildingPlace.PlacedBuilding;

                if (!building) continue;

                foreach (var electricible in building.GetComponents<IElectricible>()) {
                    if (electricible.CanSpendElectricity()) {
                        currentElectricityToDrain += electricible.GetElectricityConsumption();
                    }
                }
            }
        }

        if (currentElectricityToDrain < 1) return;

        int amount = (int)currentElectricityToDrain;
        SpendElectricity(amount);
    }

    private void SpendElectricity(int amoount)
    {
        int id = (int)ItemID.Electricity;
        cityStorage.Inventory.RemoveItemAmount(id, amoount);
        currentElectricityToDrain = 0f;
    }
}