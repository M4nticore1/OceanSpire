using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ElevatorCabinConstruction : BuildingConstruction
{
    public int FloorIndex => ((TowerBuilding)ownedBuilding).floorIndex;
    public int PlaceIndex => ((TowerBuilding)ownedBuilding).placeIndex;

    public List<EntityCityNavigator> goingForWaitingPassengers = new List<EntityCityNavigator>();
    public List<EntityCityNavigator> waitingPassengers = new List<EntityCityNavigator>();
    public List<EntityCityNavigator> goingToRidingPassengers = new List<EntityCityNavigator>();
    public List<EntityCityNavigator> ridingPassengers = new List<EntityCityNavigator>();

    public bool isMoving { get; private set; } = false;
    public int startFloorIndex { get; private set; } = 0;
    public int nextFloorIndex { get; private set; } = 0;

    private float moveSpeed => ((ownedBuilding.GetComponent<ElevatorModule>().LevelData) as ElevatorModuleLevelData).ElevatorMoveSpeed;
    private Vector3 moveDirection = Vector3.zero;

    private TimerHandle startMovingTimerHandle = new TimerHandle();
    private const float delayToStartMoving = 1f;

    public ElevatorModule OwnedElevator => ownedBuilding.GetComponent<ElevatorModule>();
    public static event System.Action<ElevatorCabinConstruction> onElevatorPlatformStopped;
    public static event System.Action<ElevatorCabinConstruction> onElevatorPlatformChangedFloor;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {

    }

    private void Update()
    {
        if (isMoving)
        {
            float speed = moveSpeed * Time.deltaTime;
            Move(moveDirection, speed);
            SetOwnedBuilding(GetFloorIndexByPosition());

            if (FloorIndex == nextFloorIndex)
                StopMoving();
        }
    }

    private void StartMovingToFloor(int targetFloorIndex)
    {
        if (targetFloorIndex == FloorIndex) return;

        isMoving = true;
        startFloorIndex = FloorIndex;

        if (targetFloorIndex > FloorIndex)
            moveDirection = Vector3.up;
        else if (targetFloorIndex < FloorIndex)
            moveDirection = Vector3.down;
    }

    private void StartMovingToFloorTimer()
    {
        TimerManager.StartTimer(startMovingTimerHandle, delayToStartMoving, () => StartMovingToFloor(GetNextFloor()));
    }

    private void RemoveMovingToFloorTimer()
    {
        TimerManager.RemoveTimer(startMovingTimerHandle);
    }

    private void StopMoving()
    {
        isMoving = false;
        transform.position = new Vector3(transform.position.x, buildingsManager.BuiltFloors[FloorIndex].transform.position.y, transform.position.z);
        OnStopMoving();
    }

    private void OnStopMoving()
    {
        // Stop entities riding
        foreach (var rider in ridingPassengers.ToArray()) {
            rider.OnCurrentElevatorStoppedMoving();
        }
        foreach (var waiter in waitingPassengers.ToArray()) {
            waiter.OnCurrentElevatorStoppedMoving();
        }

        // Continue riding to next floor
        if (ridingPassengers.Count > 0 || waitingPassengers.Count > 0) {
            StartMovingToFloorTimer();
        }
    }

    private int GetNextFloor()
    {
        if (goingToRidingPassengers.Count > 0) {
            return FloorIndex;
        }

        if (ridingPassengers.Count > 0) {
            foreach (var rider in ridingPassengers) {
                if (rider.currentPathBuilding) {
                    nextFloorIndex = ((TowerBuilding)rider.currentPathBuilding).floorIndex;
                    break;
                }
            }

            if (ridingPassengers.Count < ownedBuilding.LevelData.maxResidentsCount && waitingPassengers.Count > 0) {
                foreach (var waiter in waitingPassengers) {
                    if (nextFloorIndex < FloorIndex && waiter.floorIndex < FloorIndex) {
                        nextFloorIndex = math.max(nextFloorIndex, waiter.floorIndex);
                    }
                    else if (nextFloorIndex > FloorIndex && waiter.floorIndex > FloorIndex) {
                        nextFloorIndex = math.min(nextFloorIndex, waiter.floorIndex);
                    }
                }
            }
            else {
                foreach (var rider in ridingPassengers) {
                    int pathFloor = ((TowerBuilding)rider.currentPathBuilding).floorIndex;
                    if (nextFloorIndex < FloorIndex && pathFloor < FloorIndex) {
                        nextFloorIndex = math.max(nextFloorIndex, pathFloor);
                    }
                    else if (nextFloorIndex > FloorIndex && pathFloor > FloorIndex) {
                        nextFloorIndex = math.min(nextFloorIndex, pathFloor);
                    }
                }
            }
        }
        else if (waitingPassengers.Count > 0) {
            nextFloorIndex = waitingPassengers[0].floorIndex;
        }
        else {
            nextFloorIndex = FloorIndex;
        }

        return nextFloorIndex;
    }

    private void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;

        for (int i = 0; i < ridingPassengers.Count; i++)
            ridingPassengers[i].OnElevatorMoving(direction, speed);
    }

    public void AddPassenger(EntityCityNavigator passenger)
    {
        switch (passenger.followingPathState) {
            case FollowingPathState.GoingToWaiting:
                goingForWaitingPassengers.Add(passenger);
                break;
            case FollowingPathState.Waiting:
                waitingPassengers.Add(passenger);
                if (isMoving)
                    StartMovingToFloor(GetNextFloor());
                else
                    StartMovingToFloorTimer();
                break;
            case FollowingPathState.GoingToRiding:
                goingToRidingPassengers.Add(passenger);
                RemoveMovingToFloorTimer();
                break;
            case FollowingPathState.Riding:
                OnAddedRider(passenger);
                break;

        }
    }

    private void OnAddedRider(EntityCityNavigator passenger)
    {
        ridingPassengers.Add(passenger);
        StartMovingToFloorTimer();
    }

    public void RemovePassenger(EntityCityNavigator passenger)
    {
        switch (passenger.followingPathState) {
            case FollowingPathState.GoingToWaiting:
                goingForWaitingPassengers.Remove(passenger);
                break;
            case FollowingPathState.Waiting:
                waitingPassengers.Remove(passenger);
                break;
            case FollowingPathState.GoingToRiding:
                goingToRidingPassengers.Remove(passenger);
                break;
            case FollowingPathState.Riding:
                OnRemovedRider(passenger);
                break;

        }
    }

    private void OnRemovedRider(EntityCityNavigator passenger)
    {
        ridingPassengers.Remove(passenger);
        if (ridingPassengers.Count > 0)
            TimerManager.ResetTimer(startMovingTimerHandle);
        else
            TimerManager.RemoveTimer(startMovingTimerHandle);
    }

    private void OnEntityStopped(EntityCityNavigator entity)
    {
        if (entity.IsRidingOnElevator && entity.CurrentElevator == OwnedElevator) {
            
        }
        else if (entity.IsWaitingForElevator) {
            StartMovingToFloor(GetNextFloor());
        }
    }

    public void SetOwnedBuilding(int newFloorIndex)
    {
        if (newFloorIndex != FloorIndex && newFloorIndex >= 0) {
            ownedBuilding = buildingsManager.BuiltFloors[newFloorIndex].RoomBuildingPlaces[PlaceIndex].PlacedBuilding;
            foreach (var npc in ridingPassengers.ToArray()) {
                npc.OnCurrentElevatorChangedFloor();
            }
        }
    }

    private int GetFloorIndexByPosition()
    {
        int floorIndex = 0;

        if (nextFloorIndex >= this.FloorIndex) {
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
}
