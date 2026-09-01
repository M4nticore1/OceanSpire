using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElevatorCabinsManager : MonoBehaviour
{
    [SerializeField] private BuildingsLoader buildingsLoader;

    private List<ElevatorCabinConstruction> elevatorCabins = new();
    public IReadOnlyList<ElevatorCabinConstruction> ElevatorCabins => elevatorCabins;

    private Coroutine UpdateElevatorCabinCoroutine;

    private void OnEnable()
    {
        Building.OnBuildingInited += HandleBuildingInited;
        Building.OnBuildingConstructionFinished += HandleBuildingConstructionFinished;
        Building.OnBuildingUpgradeFinished += HandleBuildingUpgradeFinished;
        Building.OnBuildingDemolished += HandleBuildingDemolished;

        BuildingConstruction.OnBuildingConstructionInited += HandleBuildingConstructionInited;
        BuildingConstruction.OnBuildingConstructionDemolished += HandleBuildingConstructionDemolished;
    }

    private void OnDisable()
    {
        Building.OnBuildingInited -= HandleBuildingInited;
        Building.OnBuildingConstructionFinished -= HandleBuildingConstructionFinished;
        Building.OnBuildingUpgradeFinished -= HandleBuildingUpgradeFinished;
        Building.OnBuildingDemolished -= HandleBuildingDemolished;

        BuildingConstruction.OnBuildingConstructionInited -= HandleBuildingConstructionInited;
        BuildingConstruction.OnBuildingConstructionDemolished -= HandleBuildingConstructionDemolished;
    }

    private void HandleBuildingInited(Building building)
    {
        if (ShouldIgnoreEvents()) return;
        if (building == null) return;
        if (building.ConstructionComponent.ConstructionFinishTime != null) return;

        RunUpdateElevatorCabin(building);
    }

    private void HandleBuildingConstructionFinished(Building building)
    {
        if (ShouldIgnoreEvents()) return;

        RunUpdateElevatorCabin(building);
    }

    private void HandleBuildingUpgradeFinished(Building building)
    {
        if (ShouldIgnoreEvents()) return;

        RunUpdateElevatorCabin(building);
    }

    private void HandleBuildingDemolished(Building building)
    {
        if (ShouldIgnoreEvents()) return;

        if (building == null) return;

        var elevator = building.GetComponent<ElevatorModule>();
        if (elevator == null) return;

        var elevatorCabin = elevator.SpawnedElevatorCabin;
        if (elevatorCabin == null) return;

        int elevatorIndex = 0;
        var connectedBuildings = elevator.OwnedTowerBuilding.ConnectedBuildingsEnumerable().ToArray();
        if (connectedBuildings.Length > 0) {
            for (int i = connectedBuildings.Length - 1; i >= 0; i--) {
                var connectedBuilding = connectedBuildings[i];
                if (connectedBuilding == null) continue;

                var connectedElevator = connectedBuilding.GetComponent<ElevatorModule>();
                if (connectedElevator == null) continue;

                if (elevatorIndex == 0) {
                    elevatorCabin.SetOwnedBuilding(connectedBuilding);
                    elevatorCabin.ApplyOwnedBuildingPosition();
                }
                else {
                    var cabin = TryCreateCabin(connectedElevator);
                    connectedElevator.SetCabin(cabin);
                }

                UpdateElevatorNetworkCabins(connectedElevator);
                elevatorIndex++;
            }
        }
        else if (elevatorCabin) {
            elevatorCabin.Demolish();
        }
    }

    private void RunUpdateElevatorCabin(Building building)
    {
        if (UpdateElevatorCabinCoroutine == null) {
            UpdateElevatorCabinCoroutine = StartCoroutine(UpdateElevatorCabinEndOfFrame(building));
        }
    }

    private void UpdateElevatorCabin(Building building)
    {
        if (building == null) return;

        var elevator = building.GetComponent<ElevatorModule>();
        if (elevator == null) return;

        var elevatorCabin = TryGetNetworkCabin(elevator);
        if (elevatorCabin != null) {
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
            Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Owned Building is not valid at {cabinConstruction}!");
            return;
        }

        var elevator = ownedBuilding.GetComponent<ElevatorModule>();
        if (elevator == null) {
            Debug.LogError($"[{nameof(ElevatorCabinsManager)}] Elevator Module is not valid at {ownedBuilding}!");
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

            var networkCabin = networkElevator.SpawnedElevatorCabin;
            if (networkCabin != null && networkCabin != elevator.SpawnedElevatorCabin) {
                networkCabin.Demolish();
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

    private IEnumerator UpdateElevatorCabinEndOfFrame(Building building)
    {
        yield return new WaitForEndOfFrame();

        UpdateElevatorCabinCoroutine = null;
        UpdateElevatorCabin(building);
    }
}