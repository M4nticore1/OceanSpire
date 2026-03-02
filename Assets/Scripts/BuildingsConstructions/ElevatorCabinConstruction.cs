using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ElevatorCabinConstruction : BuildingConstruction
{
    public int floorIndex => ((TowerBuilding)ownedBuilding).floorIndex;
    public int placeIndex => ((TowerBuilding)ownedBuilding).placeIndex;

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

            if (floorIndex == nextFloorIndex)
                StopMoving();
        }
    }

    private void StartMovingToFloor(int targetFloorIndex)
    {
        if (targetFloorIndex == floorIndex) return;

        isMoving = true;
        startFloorIndex = floorIndex;

        if (targetFloorIndex > floorIndex)
            moveDirection = Vector3.up;
        else if (targetFloorIndex < floorIndex)
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

        // Correct position.
        transform.position = new Vector3(transform.position.x, CityManager.Instance.BuiltFloors[floorIndex].transform.position.y, transform.position.z);

        // Stop entities riding.
        foreach (var rider in ridingPassengers.ToArray()) {
            rider.OnCurrentElevatorStoppedMoving();
        }
        foreach (var waiter in waitingPassengers.ToArray()) {
            //if (ridingPassengers.Count + goingToRidingPassengers.Count >= ownedBuilding.LevelData.maxResidentsCount)
            //    break;
            waiter.OnCurrentElevatorStoppedMoving();
        }

        // Continue riding to next floor.
        if (ridingPassengers.Count > 0 || waitingPassengers.Count > 0) {
            StartMovingToFloorTimer();
        }
    }

    private int GetNextFloor()
    {
        if (goingToRidingPassengers.Count > 0) {
            return floorIndex;
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
                    if (nextFloorIndex < floorIndex && waiter.floorIndex < floorIndex) {
                        nextFloorIndex = math.max(nextFloorIndex, waiter.floorIndex);
                    }
                    else if (nextFloorIndex > floorIndex && waiter.floorIndex > floorIndex) {
                        nextFloorIndex = math.min(nextFloorIndex, waiter.floorIndex);
                    }
                }
            }
            else {
                foreach (var rider in ridingPassengers) {
                    int pathFloor = ((TowerBuilding)rider.currentPathBuilding).floorIndex;
                    if (nextFloorIndex < floorIndex && pathFloor < floorIndex) {
                        nextFloorIndex = math.max(nextFloorIndex, pathFloor);
                    }
                    else if (nextFloorIndex > floorIndex && pathFloor > floorIndex) {
                        nextFloorIndex = math.min(nextFloorIndex, pathFloor);
                    }
                }
            }
        }
        else if (waitingPassengers.Count > 0) {
            nextFloorIndex = waitingPassengers[0].floorIndex;
        }
        else {
            nextFloorIndex = floorIndex;
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
        if (entity.IsRidingOnElevator && entity.currentElevator == OwnedElevator) {
            
        }
        else if (entity.IsWaitingForElevator) {
            StartMovingToFloor(GetNextFloor());
        }
    }

    public void SetOwnedBuilding(int newFloorIndex)
    {
        if (newFloorIndex != floorIndex && newFloorIndex >= 0) {
            ownedBuilding = CityManager.Instance.BuiltFloors[newFloorIndex].roomBuildingPlaces[placeIndex].PlacedBuilding;
            foreach (var npc in ridingPassengers.ToArray()) {
                npc.OnCurrentElevatorChangedFloor();
            }
        }
    }

    private int GetFloorIndexByPosition()
    {
        int floorIndex = 0;

        if (nextFloorIndex >= this.floorIndex) {
            floorIndex = (int)((transform.position.y - CityManager.firstFloorHeight) / CityManager.floorHeight);
            if (floorIndex < startFloorIndex)
                floorIndex = startFloorIndex;
        }
        else {
            floorIndex = (int)((transform.position.y - CityManager.firstFloorHeight + CityManager.floorHeight) / CityManager.floorHeight);
            if (floorIndex > startFloorIndex)
                floorIndex = startFloorIndex;
        }
        return floorIndex;
    }
}
