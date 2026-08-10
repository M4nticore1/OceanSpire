using UnityEngine;

public class ElevatorModule : BuildingModule, IElectricible
{
    public ElevatorModuleLevelData ElevatorLevelData => LevelData as ElevatorModuleLevelData;

    [SerializeField] private float electricityConsumption = 0f;
    public float ElectricityConsumption => electricityConsumption;

    [field: SerializeField] public ElevatorCabinConstruction SpawnedElevatorCabin { get; private set; }

    // Subscribe
    protected override void Subscribe()
    {

    }

    protected override void Unsubscribe()
    {

    }

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
        if (!SpawnedElevatorCabin) {
            Debug.LogError($"[{nameof(ElevatorModule)}] Spawned Elevator Cabin is not valid!");
            return false;
        }

        return true;
    }

    public bool IsPossibleToEnter()
    {
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

    //public Transform GetCabinRidingTransform()
    //{
    //    int ridersCount = SpawnedElevatorCabin.RidingPassengers.Count;
    //    int goingToRidingCount = SpawnedElevatorCabin.GoingToRidingPassengers.Count;
    //    int totalPassengers = ridersCount + goingToRidingCount;

    //    int length = SpawnedElevatorCabin.BuildingInteractions.Length;
    //    if (length > 0 && totalPassengers > 0) {
    //        int index = (totalPassengers - 1) % length;

    //        return SpawnedElevatorCabin.GetInteraction(index).GetWaypoint(0).transform;
    //    }

    //    return transform;
    //}

    public Transform GetCabinRidingTransform(ElevatorPassenger elevatorPassenger)
    {
        if (!CanGetSpawnedCabin()) return null;

        var interaction = SpawnedElevatorCabin.GetInteractPoint(elevatorPassenger.CityNavigator);
        if (interaction == null) {
            Debug.LogError($"[{nameof(ElevatorModule)}] Interaction is not valid", this);
            return null;
        }

        var waypoint = interaction.GetWaypoint(0);
        if (waypoint == null) {
            Debug.LogError($"[{nameof(ElevatorModule)}] Waypoint is not valid", this);
            return null;
        }

        return waypoint.Transform;
    }

    public float GetElectricityConsumption()
    {
        return electricityConsumption;
    }

    public bool ShouldSpendElectricity()
    {
        return SpawnedElevatorCabin && SpawnedElevatorCabin.IsMoving && SpawnedElevatorCabin.FloorIndex == (OwnedBuilding as TowerBuilding).FloorIndex;
    }

    public void SetCabin(ElevatorCabinConstruction cabin)
    {
        if (!cabin) {
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