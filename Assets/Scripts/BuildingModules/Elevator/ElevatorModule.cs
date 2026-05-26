using UnityEngine;

public class ElevatorModule : BuildingModule, IElectricible
{
    public ElevatorModuleLevelData ElevatorLevelData => LevelData as ElevatorModuleLevelData;

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    public ElevatorCabinConstruction SpawnedElevatorCabin;

    // Subscribe
    protected override void Subscribe()
    {

    }

    protected override void Unsubscribe()
    {

    }

    // Passengers
    public void AddPassenger(CreatureCityNavigator passenger)
    {
        switch (passenger.FollowingPathState) {
            case FollowingPathState.GoingToWaiting:
                SpawnedElevatorCabin.AddGoingToWaitingPassenger(passenger);
                break;
            case FollowingPathState.Waiting:
                SpawnedElevatorCabin.AddWaitingPassenger(passenger);
                break;
            case FollowingPathState.GoingToRiding:
                SpawnedElevatorCabin.AddGoingToRidingPassenger(passenger);
                break;
            case FollowingPathState.Riding:
                SpawnedElevatorCabin.AddRidingPassenger(passenger);
                break;
        }
    }

    public void RemovePassenger(CreatureCityNavigator passenger)
    {
        switch (passenger.FollowingPathState) {
            case FollowingPathState.GoingToWaiting:
                SpawnedElevatorCabin.RemoveGoingToWaitingPassenger(passenger);
                break;
            case FollowingPathState.Waiting:
                SpawnedElevatorCabin.RemoveWaitingPassenger(passenger);
                break;
            case FollowingPathState.GoingToRiding:
                SpawnedElevatorCabin.RemoveGoingToRidingPassenger(passenger);
                break;
            case FollowingPathState.Riding:
                SpawnedElevatorCabin.RemoveRidingPassenger(passenger);
                break;
        }
    }

    public bool IsPossibleToEnter()
    {
        if (SpawnedElevatorCabin.IsMoving) return false;
        if (SpawnedElevatorCabin.OwnedElevator.OwnedTowerBuilding.FloorIndex != OwnedTowerBuilding.FloorIndex) return false;
        if (SpawnedElevatorCabin.RidingPassengers.Count + SpawnedElevatorCabin.GoingToRidingPassengers.Count >= OwnedBuilding.LevelData.MaxHumansCount) return false;

        return true;
    }

    public bool IsPossibleToExit()
    {
        if (SpawnedElevatorCabin.IsMoving) return false;

        return true;
    }

    public Transform GetCabinRidingTransform()
    {
        int ridersCount = SpawnedElevatorCabin.RidingPassengers.Count;
        int goingToRidingCount = SpawnedElevatorCabin.GoingToRidingPassengers.Count;

        int length = SpawnedElevatorCabin.BuildingInteractions.Length;
        if (length > 0) {
            int index = ((ridersCount > 0 ? (ridersCount - 1) : 0) + (goingToRidingCount > 0 ? (goingToRidingCount - 1) : 0)) % length;
            return SpawnedElevatorCabin.BuildingInteractions[index].waypoints[0].transform;
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
        return SpawnedElevatorCabin.IsMoving && SpawnedElevatorCabin && SpawnedElevatorCabin.FloorIndex == (OwnedBuilding as TowerBuilding).FloorIndex;
    }

    public void SetCabin(ElevatorCabinConstruction cabin)
    {
        if (!cabin) return;
        if (cabin == SpawnedElevatorCabin) return;

        SpawnedElevatorCabin = cabin;
    }

    public ElevatorCabinConstruction GetCabinConstructionPrefab()
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;

        if (ownedTowerBuilding.BuildingPosition == BuildingPosition.Straight) {
            return ElevatorLevelData.ElevatorPlatformStraight;
        }
        else {
            return ElevatorLevelData.ElevatorPlatformCorner;
        }

    }
}