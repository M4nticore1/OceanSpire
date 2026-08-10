using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElevatorCabinsManager : MonoBehaviour
{
    [SerializeField] private BuildingsLoader buildingsLoader;

    private List<ElevatorCabinConstruction> elevatorCabins = new();
    public IReadOnlyList<ElevatorCabinConstruction> ElevatorCabins => elevatorCabins;

    private void OnEnable()
    {
        Building.OnBuildingInited += HandleBuildingInited;
        Building.OnBuildingUpgradeFinished += HandleBuildingConstructionFinished;
        Building.OnBuildingDemolished += HandleBuildingDemolished;

        BuildingConstruction.OnBuildingConstructionInited += HandleBuildingConstructionInited;
        BuildingConstruction.OnBuildingConstructionDemolished += HandleBuildingConstructionDemolished;
    }

    private void OnDisable()
    {
        Building.OnBuildingInited -= HandleBuildingInited;
        Building.OnBuildingUpgradeFinished -= HandleBuildingConstructionFinished;
        Building.OnBuildingDemolished -= HandleBuildingDemolished;

        BuildingConstruction.OnBuildingConstructionInited -= HandleBuildingConstructionInited;
        BuildingConstruction.OnBuildingConstructionDemolished -= HandleBuildingConstructionDemolished;
    }

    private void HandleBuildingInited(Building building)
    {
        if (ShouldIgnoreEvents()) return;
        if (building == null) return;
        if (building.ConstructionComponent.ConstructionFinishTime != null) return;

        UpdateElevatorCabin(building);
    }

    private void HandleBuildingConstructionFinished(Building building)
    {
        if (ShouldIgnoreEvents()) return;

        UpdateElevatorCabin(building);
    }

    private void HandleBuildingDemolished(Building building)
    {
        if (ShouldIgnoreEvents()) return;

        if (building == null) return;

        var elevator = building.GetComponent<ElevatorModule>();
        if (elevator == null) return;

        var elevatorCabin = elevator.SpawnedElevatorCabin;
        if (elevatorCabin == null) return;

        var connectedBuildings = elevator.OwnedTowerBuilding.ConnectedBuildingsEnumerable().ToArray();

        if (connectedBuildings.Length > 0) {
            for (int i = connectedBuildings.Length - 1; i >= 0; i--) {
                var connectedBuilding = connectedBuildings[i];
                var connectedElevator = connectedBuilding.GetComponent<ElevatorModule>();

                if (i == 0) {
                    elevatorCabin.SetOwnedBuilding(connectedBuilding);
                }
                else {
                    connectedElevator.SetCabin(TryCreateCabin(connectedElevator));
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
        if (building == null) {
            Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Building is not valid!");
            return;
        }

        var elevator = building.GetComponent<ElevatorModule>();
        if (elevator == null) return;

        var elevatorCabin = TryGetNetworkCabin(elevator);

        if (elevatorCabin) {
            elevator.SetCabin(elevatorCabin);
            UpdateElevatorNetworkCabins(elevator);
        }
        else {
            TryCreateCabin(elevator);
        }
    }

    private void HandleBuildingConstructionInited(BuildingConstruction buildingConstruction)
    {
        if (buildingConstruction == null) return;

        var cabinConstruction = buildingConstruction as ElevatorCabinConstruction;
        if (cabinConstruction == null) return;

        var ownedBuilding = cabinConstruction.OwnedBuilding;
        if (ownedBuilding == null) {
            Debug.LogError($"[{nameof(ElevatorCabinsManager)}] COwned Building is not valid!");
            return;
        }

        var elevator = ownedBuilding.GetComponent<ElevatorModule>();
        if (elevator == null) {
            Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Elevator Module is not valid!");
            return;
        }

        elevator.SetCabin(cabinConstruction);
        UpdateElevatorNetworkCabins(elevator);

        elevatorCabins.Add(cabinConstruction);
    }

    private void HandleBuildingConstructionDemolished(BuildingConstruction buildingConstruction)
    {
        if (buildingConstruction == null) return;

        var cabinConstruction = buildingConstruction as ElevatorCabinConstruction;
        if (cabinConstruction == null) return;

        elevatorCabins.Remove(cabinConstruction);
    }

    public void UpdateElevatorNetworkCabins(ElevatorModule elevator)
    {
        foreach (var networkBuilding in elevator.OwnedTowerBuilding.GetNetworkBuildings()) {
            if (networkBuilding == null) {
                Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Network Building is not valid!");
                continue;
            }

            var networkElevator = networkBuilding.GetComponent<ElevatorModule>();
            if (networkElevator == null) {
                Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Network Elevator is not valid!");
                continue;
            }

            if (networkElevator.SpawnedElevatorCabin != null && networkElevator.SpawnedElevatorCabin != elevator.SpawnedElevatorCabin) {
                networkElevator.SpawnedElevatorCabin.Demolish();
            }

            networkElevator.SetCabin(elevator.SpawnedElevatorCabin);
        }
    }

    private ElevatorCabinConstruction TryCreateCabin(ElevatorModule elevator)
    {
        if (elevator == null) {
            Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Elevator is not valid!");
            return null;
        }

        if (elevator.SpawnedElevatorCabin != null) return null;

        var data = new BuildingConstructionData()
        {
            OwnedBuildingInstanceId = elevator.OwnedTowerBuilding.InstanceId.GetGuid()
        };

        var cabin = ConstructionFactory.CreateConstruction(elevator.GetCabinConstructionPrefab(), elevator.OwnedTowerBuilding.transform, data);
        return cabin;
    }

    private ElevatorCabinConstruction TryGetNetworkCabin(ElevatorModule elevator)
    {
        foreach (var connected in elevator.OwnedTowerBuilding.ConnectedBuildingsEnumerable().Reverse()) {
            if (connected == null) {
                Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Connected Building is not valid!");
                continue;
            }

            var connectedElevator = connected.GetComponent<ElevatorModule>();
            if (connectedElevator == null) {
                Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Connected Elevator is not valid!");
                continue;
            }

            if (connectedElevator.SpawnedElevatorCabin == null) continue;

            return connectedElevator.SpawnedElevatorCabin;
        }

        return null;
    }

    private bool ShouldIgnoreEvents()
    {
        if (WorldSaveHandler.Instance == null) return false;

        return !buildingsLoader.IsLoaded && WorldSaveHandler.Instance.CurrentWorldData != null;
    }
}