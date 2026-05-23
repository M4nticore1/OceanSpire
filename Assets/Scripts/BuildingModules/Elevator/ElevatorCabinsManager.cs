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
        Building.OnBuildingInited += OnBuildingInited;
        Building.OnBuildingConstructionFinished += OnBuildingConstructionFinished;
        Building.OnBuildingDemolished += OnBuildingDemolished;
        BuildingConstruction.OnBuildingConstructionInited += OnBuildingConstructionInited;
        BuildingConstruction.OnBuildingConstructionDemolished += OnBuildingConstructionDemolished;
    }

    private void OnDisable()
    {
        Building.OnBuildingInited -= OnBuildingInited;
        Building.OnBuildingConstructionFinished -= OnBuildingConstructionFinished;
        Building.OnBuildingDemolished -= OnBuildingDemolished;
        BuildingConstruction.OnBuildingConstructionInited -= OnBuildingConstructionInited;
        BuildingConstruction.OnBuildingConstructionDemolished -= OnBuildingConstructionDemolished;
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

                UpdateElevatorNetworkCabins(connectedElevator);
            }
        }
        else if (elevatorCabin) {
            elevatorCabin.Demolish();
        }
    }

    private void UpdateElevatorCabin(Building building)
    {
        var elevator = building.GetComponent<ElevatorModule>();
        if (!elevator) return;

        var elevatorCabin = TryGetNetworkCabin(elevator);

        if (elevatorCabin) {
            elevator.SetCabin(elevatorCabin);
            UpdateElevatorNetworkCabins(elevator);
        }
        else {
            CreateCabin(elevator);
        }
    }

    private void OnBuildingConstructionInited(BuildingConstruction buildingConstruction)
    {
        var cabinConstruction = buildingConstruction as ElevatorCabinConstruction;
        if (!cabinConstruction) return;

        var elevator = cabinConstruction.OwnedBuilding.GetComponent<ElevatorModule>();
        if (!elevator) return;

        elevator.SetCabin(cabinConstruction);
        UpdateElevatorNetworkCabins(elevator);

        elevatorCabins.Add(cabinConstruction);
    }

    private void OnBuildingConstructionDemolished(BuildingConstruction buildingConstruction)
    {
        var cabinConstruction = buildingConstruction as ElevatorCabinConstruction;
        if (!cabinConstruction) return;

        elevatorCabins.Remove(cabinConstruction);
    }

    public void UpdateElevatorNetworkCabins(ElevatorModule elevator)
    {
        foreach (var networkBuilding in elevator.OwnedTowerBuilding.GetNetworkBuildings()) {
            var networkElevator = networkBuilding.GetComponent<ElevatorModule>();

            if (networkElevator.SpawnedElevatorCabin && networkElevator.SpawnedElevatorCabin != elevator.SpawnedElevatorCabin)
                networkElevator.SpawnedElevatorCabin.Demolish();

            networkElevator.SetCabin(elevator.SpawnedElevatorCabin);
        }
    }

    private ElevatorCabinConstruction CreateCabin(ElevatorModule elevator)
    {
        BuildingConstructionData data = new BuildingConstructionData()
        {
            BuildingInstanceId = elevator.OwnedTowerBuilding.InstanceId.Id
        };

        var cabin = ConstructionFactory.CreateConstruction(elevator.GetCabinConstructionPrefab(), elevator.OwnedTowerBuilding.transform, data);
        return cabin;
    }

    private ElevatorCabinConstruction TryGetNetworkCabin(ElevatorModule elevator)
    {
        foreach (var connected in elevator.OwnedTowerBuilding.ConnectedBuildingsEnumerable().Reverse()) {
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