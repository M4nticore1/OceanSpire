using System.Linq;
using UnityEngine;

public class ElevatorModule : BuildingModule, IElectricible, INeighborBuildingsListener
{
    public TowerBuilding OwnedTowerBuilding => OwnedBuilding as TowerBuilding;

    private ElevatorModuleLevelData ElevatorLevelData => LevelData as ElevatorModuleLevelData;

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    public ElevatorCabinConstruction spawnedElevatorCabin { get; private set; }

    private bool IsMoving => spawnedElevatorCabin ? spawnedElevatorCabin.isMoving : false;

    protected override void OnInit()
    {
        TryCreateCabin();
    }

    // Subscribe
    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.onConstructionStarted += OnConstructionStarted;
        OwnedBuilding.onConstructionFinished += OnConstructionFinished;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.onConstructionStarted -= OnConstructionStarted;
        OwnedBuilding.onConstructionFinished -= OnConstructionFinished;
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
    public void AddPassenger(CreatureCityNavigator passenger)
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

    public void RemovePassenger(CreatureCityNavigator passenger)
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
        if (spawnedElevatorCabin.isMoving) return false;
        if (spawnedElevatorCabin.OwnedElevator.OwnedTowerBuilding.FloorIndex != OwnedTowerBuilding.FloorIndex) return false;
        if (spawnedElevatorCabin.ridingPassengers.Count + spawnedElevatorCabin.goingToRidingPassengers.Count >= OwnedBuilding.LevelData.maxResidentsCount) return false;

        return true;
    }

    public bool IsPossibleToExit()
    {
        if (spawnedElevatorCabin.isMoving) return false;

        return true;
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

    public bool ShouldSpendElectricity()
    {
        return IsMoving && spawnedElevatorCabin && spawnedElevatorCabin.FloorIndex == (OwnedBuilding as TowerBuilding).FloorIndex;
    }

    // Construction
    private void OnConstructionStarted()
    {

    }

    private void OnConstructionFinished()
    {
        TryCreateCabin();
    }

    private void TryCreateCabin()
    {
        if (OwnedBuilding.ConstructionComponent.IsUnderConstruction) return;

        ElevatorCabinConstruction cabin = TryGetNetworkElevatorCabin();

        if (cabin) {
            SetCabin(cabin);
        }
        else {
            CreateCabin();
        }
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

        if (ownedTowerBuilding.BuildingPosition == BuildingPosition.Straight) {
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
