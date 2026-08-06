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
        Building.OnBuildingInited += OnBuildingInited;
        Building.OnBuildingUpgradeFinished += OnBuildingConstructionFinished;
        Building.OnBuildingDemolished += OnBuildingDemolished;

        BuildingConstruction.OnBuildingConstructionInited += OnBuildingConstructionInited;
        BuildingConstruction.OnBuildingConstructionDemolished += OnBuildingConstructionDemolished;
    }

    private void OnDisable()
    {
        Building.OnBuildingInited -= OnBuildingInited;
        Building.OnBuildingUpgradeFinished -= OnBuildingConstructionFinished;
        Building.OnBuildingDemolished -= OnBuildingDemolished;

        BuildingConstruction.OnBuildingConstructionInited -= OnBuildingConstructionInited;
        BuildingConstruction.OnBuildingConstructionDemolished -= OnBuildingConstructionDemolished;
    }

    private void OnBuildingInited(Building building)
    {
        if (ShouldIgnoreEvents()) return;
        if (!building) return;
        if (building.ConstructionComponent.ConstructionFinishTime != null) return;

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

        if (!building) return;

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
        if (!building) {
            Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Building is not valid!");
            return;
        }

        var elevator = building.GetComponent<ElevatorModule>();
        if (!elevator) return;

        var elevatorCabin = TryGetNetworkCabin(elevator);

        if (elevatorCabin) {
            elevator.SetCabin(elevatorCabin);
            UpdateElevatorNetworkCabins(elevator);
        }
        else {
            TryCreateCabin(elevator);
        }
    }

    private void OnBuildingConstructionInited(BuildingConstruction buildingConstruction)
    {
        var cabinConstruction = buildingConstruction as ElevatorCabinConstruction;
        if (!cabinConstruction) return;

        var ownedBuilding = cabinConstruction.OwnedBuilding;
        if (!ownedBuilding) {
            Debug.LogError($"[{nameof(ElevatorCabinsManager)}] COwned Building is not valid!");
            return;
        }

        var elevator = ownedBuilding.GetComponent<ElevatorModule>();
        if (!elevator) {
            Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Elevator Module is not valid!");
            return;
        }

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
            if (!networkBuilding) {
                Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Network Building is not valid!");
                continue;
            }

            var networkElevator = networkBuilding.GetComponent<ElevatorModule>();
            if (!networkElevator) {
                Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Network Elevator is not valid!");
                continue;
            }

            if (networkElevator.SpawnedElevatorCabin && networkElevator.SpawnedElevatorCabin != elevator.SpawnedElevatorCabin)
                networkElevator.SpawnedElevatorCabin.Demolish();

            networkElevator.SetCabin(elevator.SpawnedElevatorCabin);
        }
    }

    private ElevatorCabinConstruction TryCreateCabin(ElevatorModule elevator)
    {
        if (elevator.SpawnedElevatorCabin) return null;

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
            if (!connected) {
                Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Connected Building is not valid!");
                continue;
            }

            var connectedElevator = connected.GetComponent<ElevatorModule>();
            if (!connectedElevator) {
                Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Connected Elevator is not valid!");
                continue;
            }

            if (!connectedElevator.SpawnedElevatorCabin) continue;

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