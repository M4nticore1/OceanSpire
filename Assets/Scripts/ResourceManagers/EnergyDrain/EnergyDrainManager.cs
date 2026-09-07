using UnityEngine;

public class EnergyDrainManager : MonoBehaviour
{
    [field: SerializeField] public float CurrentDrainAmount { get; private set; }

    private double lastCheckTime;

    private void Update()
    {
        double currentTime = Time.timeAsDouble;
        double elapsedTime = currentTime - lastCheckTime;

        if (elapsedTime < 1d)
            return;

        lastCheckTime = currentTime;

        var floors = BuildingsManager.Instance.BuiltFloors;
        for (int i = 0; i < floors.Count; i++) {
            var floorModule = floors[i];
            if (floorModule == null) continue;

            foreach (var buildingPlace in floorModule.RoomBuildingPlaces) {
                var placedBuilding = buildingPlace.PlacedBuilding;
                if (placedBuilding == null) continue;

                for (int j = 0; j < placedBuilding.BuildingModules.Length; j++) {
                    var module = placedBuilding.BuildingModules[j];
                    if (module == null || !module.ShouldSpendElectricity())
                        continue;

                    CurrentDrainAmount += (float)(module.GetElectricityConsumptionPerMinute() / 60d * elapsedTime);
                }
            }
        }

        TrySpendElectricity();
    }

    private void TrySpendElectricity()
    {
        if (CurrentDrainAmount < 1f)
            return;

        var amount = Mathf.FloorToInt(CurrentDrainAmount);
        SpendElectricity(amount);

        CurrentDrainAmount -= amount;
    }

    public void Init()
    {
        Init(EnergyDrainData.Default());
    }

    public void Init(EnergyDrainData energyDrainData)
    {
        if (energyDrainData == null) {
            Debug.LogError($"[{nameof(EnergyDrainManager)}] Energy Drain Data is not valid!");
            return;
        }

        CurrentDrainAmount = energyDrainData.DrainAmount;
        lastCheckTime = Time.timeAsDouble;
    }

    private void SpendElectricity(int amount)
    {
        CityStorage.Instance.Inventory.RemoveItemAmount(ItemID.Electricity, amount
        );
    }
}