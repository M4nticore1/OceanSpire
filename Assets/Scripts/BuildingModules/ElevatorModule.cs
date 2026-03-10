using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElevatorModule : BuildingModule, IElectricible, IOwnedBuildingListener, INeighborBuildingsListener
{
    private ElevatorModuleLevelData ElevatorLevelData => LevelData as ElevatorModuleLevelData;

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    public ElevatorCabinConstruction spawnedElevatorCabin;
    public int elevatorGroupId { get; private set; } = 0;

    private bool IsMoving => spawnedElevatorCabin.isMoving;

    protected override void OnInit()
    {
        ElevatorCabinConstruction cabin = TryGetConnectedElevatorCabin();

        if (cabin) {
            spawnedElevatorCabin = cabin;
        }
        else {
            CreateCabin();
        }
    }

    protected override void OnBuildingStartWorking()
    {

    }

    protected override void OnBuildingStopWorking()
    {

    }

    protected override void OnEnterBuilding(EntityCityNavigator navigator)
    {

    }

    protected override void OnExitBuilding(EntityCityNavigator navigator)
    {

    }

    // IOwnedBuildingListener
    public void HandleOwnedBuildingInited()
    {

    }

    public void HandleOwnedBuildingDemolished()
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;
        if (spawnedElevatorCabin && spawnedElevatorCabin.FloorIndex == ownedTowerBuilding.floorIndex) {
            DestroyCabin();
        }
    }

    // IConnectedBuildingsListener
    public void HandleNeighborBuildingInited(TowerBuilding building)
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;
        if (!building.ConnectedWith(ownedTowerBuilding)) return;

        if (building.floorIndex > ownedTowerBuilding.floorIndex) return;

        ElevatorModule initedElevator = building.GetComponent<ElevatorModule>();
        if (initedElevator.spawnedElevatorCabin == spawnedElevatorCabin) return;

        if (spawnedElevatorCabin) {
            DestroyCabin();
        }
        spawnedElevatorCabin = initedElevator.TryGetConnectedElevatorCabin();
    }

    public void HandleNeighborBuildingDemolished(TowerBuilding building)
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;
        if (spawnedElevatorCabin.FloorIndex == ownedTowerBuilding.floorIndex) return;

        spawnedElevatorCabin = TryGetConnectedElevatorCabin();
        if (spawnedElevatorCabin) return;

        CreateCabin();
    }

    // Passengers
    public void AddPassenger(EntityCityNavigator passenger)
    {
        spawnedElevatorCabin.AddPassenger(passenger);
    }

    public void RemovePassenger(EntityCityNavigator passenger)
    {
        spawnedElevatorCabin.RemovePassenger(passenger);
    }

    public bool IsPossibleToEnter()
    {
        return !spawnedElevatorCabin.isMoving && (spawnedElevatorCabin.OwnedElevator.OwnedBuilding as TowerBuilding).floorIndex == (OwnedBuilding as TowerBuilding).floorIndex && spawnedElevatorCabin.ridingPassengers.Count < OwnedBuilding.LevelData.maxResidentsCount;
    }

    public bool IsPossibleToExit()
    {
        return !spawnedElevatorCabin.isMoving;
    }

    public Transform GetCabinRidingTransform()
    {
        int ridersCount = spawnedElevatorCabin.ridingPassengers.Count;
        int goingToRidingCount = spawnedElevatorCabin.goingToRidingPassengers.Count;

        int length = spawnedElevatorCabin.BuildingInteractions.Length;
        if (length > 0) {
            int index = ((ridersCount > 0 ? (ridersCount - 1) : 0) + (goingToRidingCount > 0 ? (goingToRidingCount - 1) : 0)) % length;
            return spawnedElevatorCabin.BuildingInteractions[index].waypoints[0].transform;
        }
        else {
            return transform;
        }
    }

    public float GetElectricityConsumption()
    {
        return electricityConsumption;
    }

    public bool CanSpendElectricity()
    {
        return IsMoving && spawnedElevatorCabin.FloorIndex == (OwnedBuilding as TowerBuilding).floorIndex;
    }

    private bool TryApplyCabin()
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;
        ElevatorModule belowElevatorBuilding = ownedTowerBuilding.downBuilding?.GetComponent<ElevatorModule>();
        ElevatorModule aboveElevatorBuilding = ownedTowerBuilding.upBuilding?.GetComponent<ElevatorModule>();

        if (belowElevatorBuilding && belowElevatorBuilding.spawnedElevatorCabin) {
            elevatorGroupId = belowElevatorBuilding.elevatorGroupId;
            spawnedElevatorCabin = belowElevatorBuilding.spawnedElevatorCabin;
            return true;
        }
        else if (aboveElevatorBuilding && aboveElevatorBuilding.spawnedElevatorCabin) {
            elevatorGroupId = aboveElevatorBuilding.elevatorGroupId;
            spawnedElevatorCabin = aboveElevatorBuilding.spawnedElevatorCabin;
            return true;
        }
        return false;
    }

    private void CreateCabin()
    {
        ElevatorCabinConstruction cabinToSpawn = GetCabinConstruction();
        spawnedElevatorCabin = ConstructionFactory.CreateConstruction(cabinToSpawn, OwnedBuilding);
    }

    private void DestroyCabin()
    {
        Destroy(spawnedElevatorCabin.gameObject);
        spawnedElevatorCabin = null;
    }

    private ElevatorCabinConstruction GetCabinConstruction()
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;

        if (ownedTowerBuilding.buildingPosition == BuildingPosition.Straight) {
            return ElevatorLevelData.ElevatorPlatformStraight;
        }
        else {
            return ElevatorLevelData.ElevatorPlatformCorner;
        }

    }

    private ElevatorCabinConstruction TryGetConnectedElevatorCabin()
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;
        TowerBuilding[] connectedElevators = ownedTowerBuilding.ConnectedBuildings().ToArray();

        for (int i = connectedElevators.Count() - 1; i >= 0; i--) {
            TowerBuilding towerBuilding = connectedElevators[i];
            ElevatorModule elevator = towerBuilding.GetComponent<ElevatorModule>();
            if (!elevator) continue;

            ElevatorCabinConstruction cabin = elevator.spawnedElevatorCabin;
            if (!cabin) continue;
            if (!cabin.OwnedElevator.spawnedElevatorCabin) continue;

            return cabin;
        }
        return null;
    }
}
