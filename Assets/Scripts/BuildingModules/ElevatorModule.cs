using System.Linq;
using UnityEngine;

public class ElevatorModule : BuildingModule, IElectricible, INeighborBuildingsListener
{
    public TowerBuilding OwnedTowerBuilding => OwnedBuilding as TowerBuilding;

    private ElevatorModuleLevelData ElevatorLevelData => LevelData as ElevatorModuleLevelData;

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    public ElevatorCabinConstruction spawnedElevatorCabin;
    public int elevatorGroupId { get; private set; } = 0;

    private bool IsMoving => spawnedElevatorCabin.isMoving;

    protected override void OnInit()
    {
        ElevatorCabinConstruction cabin = TryGetNetworkElevatorCabin();

        if (cabin) {
            spawnedElevatorCabin = cabin;
        }
        else {
            CreateCabin();
        }
    }

    protected override void OnDemolish()
    {
        if (OwnedTowerBuilding.ConnectedBuildings().Count() == 0) {
            DestroyCabin();
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

    // IConnectedBuildingsListener
    public void HandleNeighborBuildingInited(TowerBuilding initedBuilding)
    {
        ElevatorModule initedElevator = initedBuilding.GetComponent<ElevatorModule>();
        if (!initedElevator) return;
        if (!initedBuilding.ConnectedWith(OwnedTowerBuilding)) return;
        if (initedElevator.spawnedElevatorCabin == spawnedElevatorCabin) return;

        if (spawnedElevatorCabin) {
            spawnedElevatorCabin.StopMoving();
            spawnedElevatorCabin.UnloadRidingPassengers();
            DestroyCabin();
        }
        SetCabin(initedElevator.spawnedElevatorCabin);
    }

    public void HandleNeighborBuildingDemolished(TowerBuilding demolishedBuilding)
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;
        if (!demolishedBuilding.ConnectedWith(ownedTowerBuilding))
            return;

        ElevatorModule demolishedElevator = demolishedBuilding.GetComponent<ElevatorModule>();
        ElevatorCabinConstruction cabin = demolishedElevator.spawnedElevatorCabin;
        TowerBuilding cabinOwnedBuilding = spawnedElevatorCabin.OwnedElevator.OwnedTowerBuilding;

        if (demolishedElevator && cabin.ownedBuilding == demolishedBuilding) {
            cabin.SetOwnedBuilding(OwnedBuilding);
        }
        else if (!ownedTowerBuilding.NetworkWith(cabinOwnedBuilding)) {
            CreateCabin();
        }

        cabin.UpdateWaitingPassengers();

        if (cabin.isMoving && !cabin.TryMoveToFloor(cabin.nextFloor)) {
            cabin.StopMoving();
        }
    }

    // Passengers
    public void AddPassenger(EntityCityNavigator passenger)
    {
        switch (passenger.followingPathState) {
            case FollowingPathState.GoingToWaiting:
                spawnedElevatorCabin.AddGoingToWaitingPassenger(passenger);
                break;
            case FollowingPathState.Waiting:
                spawnedElevatorCabin.AddWaitingPassenger(passenger);
                break;
            case FollowingPathState.GoingToRiding:
                spawnedElevatorCabin.AddGoingToRidingPassenger(passenger);
                break;
            case FollowingPathState.Riding:
                spawnedElevatorCabin.AddRidingPassenger(passenger);
                break;
        }
    }

    public void RemovePassenger(EntityCityNavigator passenger)
    {
        switch (passenger.followingPathState) {
            case FollowingPathState.GoingToWaiting:
                spawnedElevatorCabin.RemoveGoingToWaitingPassenger(passenger);
                break;
            case FollowingPathState.Waiting:
                spawnedElevatorCabin.RemoveWaitingPassenger(passenger);
                break;
            case FollowingPathState.GoingToRiding:
                spawnedElevatorCabin.RemoveGoingToRidingPassenger(passenger);
                break;
            case FollowingPathState.Riding:
                spawnedElevatorCabin.RemoveRidingPassenger(passenger);
                break;
        }
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

    private void HandleConnectedElevatorCabinChanged(ElevatorModule connectedElevator)
    {
        ElevatorCabinConstruction changedCabin = connectedElevator.spawnedElevatorCabin;
        if (spawnedElevatorCabin == changedCabin)
            return;

        SetCabin(changedCabin);
    }

    private void InvokeCabinChanged()
    {
        foreach (var building in OwnedTowerBuilding.ConnectedBuildings()) {
            ElevatorModule elevator = building.GetComponent<ElevatorModule>();
            elevator.HandleConnectedElevatorCabinChanged(this);
        }
    }

    private void SetCabin(ElevatorCabinConstruction cabin)
    {
        spawnedElevatorCabin = cabin;
        InvokeCabinChanged();
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
        ElevatorCabinConstruction cabin = ConstructionFactory.CreateConstruction(cabinToSpawn, OwnedBuilding);
        SetCabin(cabin);
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

    private ElevatorCabinConstruction TryGetNetworkElevatorCabin()
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;
        TowerBuilding[] connectedElevators = ownedTowerBuilding.GetNetworkBuildings().ToArray();

        for (int i = connectedElevators.Count() - 1; i >= 0; i--) {
            TowerBuilding towerBuilding = connectedElevators[i];
            ElevatorModule elevator = towerBuilding.GetComponent<ElevatorModule>();
            if (!elevator) continue;

            ElevatorCabinConstruction cabin = elevator.spawnedElevatorCabin;
            if (!cabin) continue;

            return cabin;
        }
        return null;
    }
}
