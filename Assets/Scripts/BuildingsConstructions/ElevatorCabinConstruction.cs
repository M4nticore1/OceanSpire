using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ElevatorCabinConstruction : BuildingConstruction
{
    public int FloorIndex => ((TowerBuilding)ownedBuilding).floorIndex;
    public int PlaceIndex => ((TowerBuilding)ownedBuilding).placeIndex;

    public List<CreatureCityNavigator> goingForWaitingPassengers { get; private set; } = new List<CreatureCityNavigator>();
    public List<CreatureCityNavigator> waitingPassengers { get; private set; } = new List<CreatureCityNavigator>();
    public List<CreatureCityNavigator> goingToRidingPassengers { get; private set; } = new List<CreatureCityNavigator>();
    public List<CreatureCityNavigator> ridingPassengers { get; private set; } = new List<CreatureCityNavigator>();

    public bool isMoving { get; private set; } = false;
    public int startFloorIndex { get; private set; } = 0;
    public int targetFloor { get; private set; } = 0;
    public int nextFloor { get; private set; } = 0;

    private float moveSpeed => ((ownedBuilding.GetComponent<ElevatorModule>().LevelData) as ElevatorModuleLevelData).ElevatorMoveSpeed;
    private Vector3 moveDirection = Vector3.zero;

    private TimerHandle startMovingTimerHandle = new TimerHandle();
    private const float delayToStartMoving = 1f;

    public ElevatorModule OwnedElevator => ownedBuilding.GetComponent<ElevatorModule>();
    public static event System.Action<ElevatorCabinConstruction> onElevatorPlatformStopped;
    public static event System.Action<ElevatorCabinConstruction> onElevatorPlatformChangedFloor;

    private void Update()
    {
        if (!isMoving) return;

        float speed = moveSpeed * Time.deltaTime;
        Move(moveDirection, speed);

        int floor = GetFloorIndexByPosition();
        ApplyOwnedBuildingByFloor(floor);
    }

    public override void SetOwnedBuilding(Building building)
    {
        base.SetOwnedBuilding(building);

        foreach (var npc in ridingPassengers.ToArray()) {
            npc.OnElevatorChangedFloor(building);
        }

        AssignTargetFloor();

        if (FloorIndex == targetFloor) {
            StopMoving();
        }
        else {
            if (!TryMoveToFloor(targetFloor)) {
                StopMoving();
            }
        }
    }

    public void SetTargetFloor(int floorIndex)
    {
        targetFloor = floorIndex;
    }

    public void SetNextFloor(int floorIndex)
    {
        nextFloor = floorIndex;
    }

    public void StopMoving()
    {
        isMoving = false;
        ApplyOwnedBuildingPosition();

        // Stop entities riding
        foreach (var rider in ridingPassengers.ToArray()) {
            rider.OnElevatorStopped();
        }
        foreach (var waiter in waitingPassengers.ToArray()) {
            waiter.OnElevatorStopped();
        }

        // Continue riding to next floor
        if (ridingPassengers.Count > 0 || waitingPassengers.Count > 0) {
            StartMovingToFloorTimer();
        }
    }

    // Waiting Passengers
    public void AddGoingToWaitingPassenger(CreatureCityNavigator passenger)
    {
        goingForWaitingPassengers.Add(passenger);
    }

    public void RemoveGoingToWaitingPassenger(CreatureCityNavigator passenger)
    {
        goingForWaitingPassengers.Remove(passenger);
    }

    // Waiting Passengers
    public void AddWaitingPassenger(CreatureCityNavigator passenger)
    {
        waitingPassengers.Add(passenger);
        if (isMoving) {
            AssignTargetFloor();
            StartMovingToFloor(targetFloor);
        }
        else {
            StartMovingToFloorTimer();
        }
    }

    public void RemoveWaitingPassenger(CreatureCityNavigator passenger)
    {
        waitingPassengers.Remove(passenger);
    }

    // Going To Riding Passengers
    public void AddGoingToRidingPassenger(CreatureCityNavigator passenger)
    {
        goingToRidingPassengers.Add(passenger);
        RemoveMovingToFloorTimer();
    }

    public void RemoveGoingToRidingPassenger(CreatureCityNavigator passenger)
    {
        goingToRidingPassengers.Remove(passenger);
    }

    // Riding Passengers
    public void AddRidingPassenger(CreatureCityNavigator passenger)
    {
        ridingPassengers.Add(passenger);
        StartMovingToFloorTimer();
    }

    public void RemoveRidingPassenger(CreatureCityNavigator passenger)
    {
        ridingPassengers.Remove(passenger);

        if (ridingPassengers.Count > 0)
            TimerManager.ResetTimer(startMovingTimerHandle);
        else
            TimerManager.RemoveTimer(startMovingTimerHandle);
    }

    // Passengers
    public void UpdateWaitingPassengers()
    {
        for (int i = waitingPassengers.Count - 1; i >= 0; i--) {
            CreatureCityNavigator navigator = waitingPassengers[i];
            int floor = navigator.floorIndex;

            if (!CanMoveToFloor(floor)) {
                RemoveWaitingPassenger(navigator);
            }
        }
    }

    public void UnloadRidingPassengers()
    {
        for (int i = ridingPassengers.Count - 1; i >= 0; i--) {
            CreatureCityNavigator passenger = ridingPassengers[i];
            passenger.SetState(FollowingPathState.GoingToWaiting);
        }
    }

    // Floor
    public bool TryMoveToFloor(int floor)
    {
        if (!CanMoveToFloor(floor))
            return false;

        StartMovingToFloor(floor);
        return true;
    }

    public bool CanMoveToFloor(int floor)
    {
        TowerBuilding targetBuilding = BuildingsManager.instance.BuiltFloors[floor].RoomBuildingPlaces[PlaceIndex].PlacedBuilding;
        if (!targetBuilding)
            return false;

        if (!targetBuilding.NetworkWith(OwnedElevator.OwnedTowerBuilding))
            return false;

        return true;
    }

    private void StartMovingToFloor(int floorIndex)
    {
        isMoving = true;
        startFloorIndex = FloorIndex;

        if (floorIndex > FloorIndex)
            moveDirection = Vector3.up;
        else if (floorIndex < FloorIndex)
            moveDirection = Vector3.down;
    }

    private void StartMovingToFloorTimer()
    {
        AssignTargetFloor();
        TimerManager.StartTimer(startMovingTimerHandle, delayToStartMoving, () => StartMovingToFloor(targetFloor));
    }

    private void RemoveMovingToFloorTimer()
    {
        TimerManager.RemoveTimer(startMovingTimerHandle);
    }

    private void AssignTargetFloor()
    {
        SetTargetFloor(CalculateTargetFloor());
        SetNextFloor(CalculateNextFloor());
    }

    private int CalculateTargetFloor()
    {
        if (goingToRidingPassengers.Count > 0) {
            return FloorIndex;
        }

        int targetFloor = FloorIndex;
        if (ridingPassengers.Count > 0) {
            foreach (var rider in ridingPassengers) {
                if (rider.currentPathBuilding) {
                    targetFloor = rider.currentPathTowerBuilding ? rider.currentPathTowerBuilding.floorIndex : FloorIndex;
                    break;
                }
            }

            if (ridingPassengers.Count < ownedBuilding.LevelData.maxResidentsCount && waitingPassengers.Count > 0) {
                foreach (var waiter in waitingPassengers) {
                    if (targetFloor < FloorIndex && waiter.floorIndex < FloorIndex) {
                        targetFloor = math.max(targetFloor, waiter.floorIndex);
                    }
                    else if (targetFloor > FloorIndex && waiter.floorIndex > FloorIndex) {
                        targetFloor = math.min(targetFloor, waiter.floorIndex);
                    }
                }
            }
            else {
                foreach (var rider in ridingPassengers) {
                    TowerBuilding pathTowerBuilding = rider.currentPathTowerBuilding;
                    if (!pathTowerBuilding)
                        continue;

                    int pathFloor = pathTowerBuilding.floorIndex;
                    if (targetFloor < FloorIndex && pathFloor < FloorIndex) {
                        targetFloor = math.max(targetFloor, pathFloor);
                    }
                    else if (targetFloor > FloorIndex && pathFloor > FloorIndex) {
                        targetFloor = math.min(targetFloor, pathFloor);
                    }
                }
            }
        }
        else if (waitingPassengers.Count > 0) {
            targetFloor = waitingPassengers[0].floorIndex;
        }
        else {
            targetFloor = FloorIndex;
        }
        return targetFloor;
    }

    private int CalculateNextFloor()
    {
        return targetFloor > FloorIndex ? FloorIndex + 1 : targetFloor < FloorIndex ? FloorIndex - 1 : FloorIndex;
    }

    private void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }

    private int GetFloorIndexByPosition()
    {
        int floorIndex = 0;

        if (targetFloor >= this.FloorIndex) {
            floorIndex = (int)((transform.position.y - BuildingsManager.FirstFloorHeight) / BuildingsManager.FloorHeight);
            if (floorIndex < startFloorIndex)
                floorIndex = startFloorIndex;
        }
        else {
            floorIndex = (int)((transform.position.y - BuildingsManager.FirstFloorHeight + BuildingsManager.FloorHeight) / BuildingsManager.FloorHeight);
            if (floorIndex > startFloorIndex)
                floorIndex = startFloorIndex;
        }
        return floorIndex;
    }

    private void ApplyOwnedBuildingByFloor(int floor)
    {
        TowerBuilding building = BuildingsManager.instance.BuiltFloors[floor].RoomBuildingPlaces[PlaceIndex].PlacedBuilding;
        if (building == ownedBuilding) return;

        SetOwnedBuilding(building);
    }
}