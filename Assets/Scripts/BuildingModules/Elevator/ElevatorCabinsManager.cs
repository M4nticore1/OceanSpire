using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElevatorCabinsManager : MonoBehaviour
{
    private List<ElevatorCabinConstruction> elevatorCabins = new();
    public IReadOnlyList<ElevatorCabinConstruction> ElevatorCabins => elevatorCabins;

    private void OnEnable()
    {
        Building.onBuildingInited += OnBuildingInited;
        Building.onBuildingConstructionFinished += OnBuildingConstructionFinished;
        Building.onBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        Building.onBuildingInited -= OnBuildingInited;
        Building.onBuildingConstructionFinished -= OnBuildingConstructionFinished;
        Building.onBuildingDemolished -= OnBuildingDemolished;
    }

    private void OnBuildingInited(Building building)
    {
        if (ShouldIgnoreEvents()) return;
        if (building.ConstructionComponent.IsUnderConstruction) return;

        UpdateElevatorCabin(building);
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        if (ShouldIgnoreEvents()) return;

        UpdateElevatorCabin(building);
    }

    private void OnBuildingDemolished(Building building)
    {
        if (ShouldIgnoreEvents()) return;

        var elevator = building.GetComponent<ElevatorModule>();
        if (!elevator) return;

        var elevatorCabin = elevator.SpawnedElevatorCabin;
        var connectedBuildings = elevator.OwnedTowerBuilding.ConnectedBuildingsEnumerable().ToArray();

        if (connectedBuildings.Length > 0) {
            for (int i = connectedBuildings.Length - 1; i >= 0; i--) {
                var connectedBuilding = connectedBuildings[i];
                var connectedElevator = connectedBuilding.GetComponent<ElevatorModule>();

                if (i == 0) {
                    elevatorCabin.SetOwnedBuilding(connectedBuilding);
                }
                else {
                    connectedElevator.SetCabin(CreateCabin(connectedElevator));
                }

                connectedElevator.UpdateNetworkCabins();
            }
        }
        else {
            elevatorCabin.Demolish();
        }
    }

    private void UpdateElevatorCabin(Building building)
    {
        var elevator = building.GetComponent<ElevatorModule>();
        if (!elevator) return;

        var elevatorCabin = TryGetNetworkCabin(elevator);

        elevator.SetCabin(elevatorCabin ? elevatorCabin : CreateCabin(elevator));
    }

    private ElevatorCabinConstruction CreateCabin(ElevatorModule elevator)
    {
        BuildingConstructionData data = new BuildingConstructionData()
        {
            BuildingInstanceId = elevator.OwnedTowerBuilding.InstanceId.Id
        };

        var cabin = ConstructionFactory.CreateConstruction(elevator.GetCabinConstructionPrefab(), elevator.OwnedTowerBuilding.transform, data);
        elevatorCabins.Add(cabin);

        return cabin;
    }

    private ElevatorCabinConstruction TryGetNetworkCabin(ElevatorModule elevator)
    {
        foreach (var connected in elevator.OwnedTowerBuilding.ConnectedBuildingsEnumerable()) {
            var connectedElevator = connected.GetComponent<ElevatorModule>();
            if (!connectedElevator.SpawnedElevatorCabin) continue;

            return connectedElevator.SpawnedElevatorCabin;
        }

        return null;
    }

    private bool ShouldIgnoreEvents()
    {
        return !BuildingsLoader.Instance.IsLoaded && WorldSaveManager.Instance.CurrentWorldData != null;
    }
}