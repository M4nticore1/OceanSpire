using UnityEngine;

public class EnergyDrainManager : MonoBehaviour
{
    [field: SerializeField] public float CurrentDrainAmount { get; private set; } = 0f;
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
                        CurrentDrainAmount += module.GetElectricityConsumption();
                    }
                }
            }
        }

        if (CurrentDrainAmount < 1f) return;

        var amount = (int)CurrentDrainAmount;
        SpendElectricity(amount);
    }

    public void Init()
    {
        Init(EnergyDrainData.Default());
    }

    public void Init(EnergyDrainData energyDrainData)
    {
        if (energyDrainData == null) {
            Debug.LogError($"[{nameof(EnergyDrainManager)}] Energy Drain Data is not valid!");
            Init(energyDrainData);
            return;
        }

        CurrentDrainAmount = energyDrainData.DrainAmount;
    }

    private void SpendElectricity(int amount)
    {
        var id = ItemID.Electricity;
        CityStorage.Instance.Inventory.RemoveItemAmount(id, amount);
        CurrentDrainAmount = 0f;
    }
}