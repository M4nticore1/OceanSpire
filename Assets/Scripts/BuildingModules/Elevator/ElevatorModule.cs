using UnityEngine;

public class ElevatorModule : BuildingModule
{
    public ElevatorModuleLevelData ElevatorLevelData => LevelData as ElevatorModuleLevelData;

    [field: SerializeField] public ElevatorCabinConstruction SpawnedElevatorCabin { get; private set; }

    // Subscribe
    protected override void Subscribe()
    {

    }

    protected override void Unsubscribe()
    {

    }

    // Building
    //public override bool ShouldBuild(BuildingPlace buildingPlace)
    //{
    //    if (!base.ShouldBuild(buildingPlace)) return false;

    //    var upPlace = buildingPlace.NeighborBuildingPlaces[Direction.Up];
    //    if (upPlace && upPlace.PlacedBuilding && upPlace.PlacedBuilding.Definition == OwnedBuilding.Definition) {
    //        return true;
    //    }

    //    var downPlace = buildingPlace.NeighborBuildingPlaces[Direction.Down];
    //    if (downPlace && downPlace.PlacedBuilding && downPlace.PlacedBuilding.Definition == OwnedBuilding.Definition) {
    //        return true;
    //    }

    //    return false;
    //}

    // Passengers
    public void AddGoingToWaitingPassenger(ElevatorPassenger elevatorPassenger)
    {
        if (!CanGetSpawnedCabin()) return;

        SpawnedElevatorCabin.AddGoingToWaitingPassenger(elevatorPassenger);
    }

    public void RemoveGoingToWaitingPassenger(ElevatorPassenger elevatorPassenger)
    {
        if (!CanGetSpawnedCabin()) return;

        SpawnedElevatorCabin.RemoveGoingToWaitingPassenger(elevatorPassenger);
    }

    public void AddWaitingPassenger(ElevatorPassenger elevatorPassenger)
    {
        if (!CanGetSpawnedCabin()) return;

        SpawnedElevatorCabin.AddWaitingPassenger(elevatorPassenger);
    }

    public void RemoveWaitingPassenger(ElevatorPassenger elevatorPassenger)
    {
        if (!CanGetSpawnedCabin()) return;

        SpawnedElevatorCabin.RemoveWaitingPassenger(elevatorPassenger);
    }

    public void AddGoingToRidingPassenger(ElevatorPassenger elevatorPassenger)
    {
        if (!CanGetSpawnedCabin()) return;

        SpawnedElevatorCabin.AddGoingToRidingPassenger(elevatorPassenger);
    }

    public void RemoveGoingToRidingPassenger(ElevatorPassenger elevatorPassenger)
    {
        if (!CanGetSpawnedCabin()) return;

        SpawnedElevatorCabin.RemoveGoingToRidingPassenger(elevatorPassenger);
    }

    public void AddRidingPassenger(ElevatorPassenger elevatorPassenger)
    {
        if (!CanGetSpawnedCabin()) return;

        SpawnedElevatorCabin.AddRidingPassenger(elevatorPassenger);
    }

    public void RemoveRidingPassenger(ElevatorPassenger elevatorPassenger)
    {
        if (!CanGetSpawnedCabin()) return;

        SpawnedElevatorCabin.RemoveRidingPassenger(elevatorPassenger);
    }

    private bool CanGetSpawnedCabin()
    {
        if (SpawnedElevatorCabin == null) {
            Debug.LogError($"[{nameof(ElevatorModule)}] Spawned Elevator Cabin is not valid!");
            return false;
        }

        return true;
    }

    public bool IsPossibleToEnter()
    {
        if (SpawnedElevatorCabin == null) {
            Debug.LogError($"[{nameof(ElevatorModule)}] Spawned Elevator Cabin is not valid!");
            return false;
        }
        if (SpawnedElevatorCabin.IsMoving) return false;
        if (SpawnedElevatorCabin.OwnedElevator.OwnedTowerBuilding.FloorIndex != OwnedTowerBuilding.FloorIndex) return false;
        if (SpawnedElevatorCabin.RidingPassengers.Count + SpawnedElevatorCabin.GoingToRidingPassengers.Count >= OwnedBuilding.LevelDefinition.MaxHumansCount) return false;

        return true;
    }

    public bool IsPossibleToExit()
    {
        if (SpawnedElevatorCabin == null) {
            Debug.LogError($"[{nameof(ElevatorModule)}] Spawned Elevator Cabin is not valid!");
            return false;
        }

        return !SpawnedElevatorCabin.IsMoving;
    }

    public Transform GetCabinRidingTransform(ElevatorPassenger elevatorPassenger)
    {
        if (!CanGetSpawnedCabin()) return transform;

        var interaction = SpawnedElevatorCabin.InteractionPointsHandler.GetInteractPoint(elevatorPassenger.CityNavigator);
        if (interaction == null) return transform;

        var waypoint = interaction.GetWaypoint(0);
        if (waypoint == null) return transform;

        return waypoint.Transform;
    }

    public override bool ShouldSpendElectricity()
    {
        return SpawnedElevatorCabin != null && SpawnedElevatorCabin.IsMoving;
    }

    public void SetCabin(ElevatorCabinConstruction cabin)
    {
        if (cabin == null) {
            Debug.LogError($"[{nameof(ElevatorModule)}] Cabin Construction is not valid!");
            return;
        }

        if (cabin == SpawnedElevatorCabin) return;

        SpawnedElevatorCabin = cabin;
    }

    public ElevatorCabinConstruction GetCabinConstructionPrefab()
    {
        var ownedTowerBuilding = OwnedBuilding as TowerBuilding;

        if (ownedTowerBuilding.BuildingPosition == BuildingPosition.Straight) {
            return ElevatorLevelData.ElevatorPlatformStraight;
        }
        else {
            return ElevatorLevelData.ElevatorPlatformCorner;
        }
    }
}