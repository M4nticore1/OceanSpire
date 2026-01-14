using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ElevatorPlatformConstruction : BuildingConstruction
{
    private CityManager cityManager = null;

    public List<Entity> goingForWaitingPassengers = new List<Entity>();
    public List<Entity> waitingPassengers = new List<Entity>();
    public List<Entity> goingToRidingPassengers = new List<Entity>();
    public List<Entity> ridingPassengers = new List<Entity>();

    public bool isMoving { get; private set; } = false;
    public int startFloorIndex { get; private set; } = 0;
    public int nextFloorIndex { get; private set; } = 0;

    private float moveSpeed => ((ElevatorLevelData)ownedBuilding.currentLevelData).ElevatorMoveSpeed;
    private Vector3 moveDirection = Vector3.zero;

    private TimerHandle startMovingTimerHandle = new TimerHandle();
    private const float delayToStartMoving = 1f;

    public ElevatorBuilding ownedElevator => ownedBuilding as ElevatorBuilding;
    public static event System.Action<ElevatorPlatformConstruction> onElevatorPlatformStopped;
    public static event System.Action<ElevatorPlatformConstruction> onElevatorPlatformChangedFloor;

    protected void Update()
    {
        if (isMoving)
        {
            SetOwnedBuilding(GetFloorIndexByPosition());

            float speed = moveSpeed * Time.deltaTime;
            Move(moveDirection, speed);

            if (floorIndex == nextFloorIndex)
                StopMoving();
        }
    }

    public override void Build(Building ownedBuilding)
    {
        base.Build(ownedBuilding);

        cityManager = FindAnyObjectByType<CityManager>();
    }

    private void StartMovingToFloor(int targetFloorIndex)
    {
        Debug.Log("StartMovingToFloor " + targetFloorIndex);
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
        Debug.Log("StartMovingToFloorTimer");
        TimerManager.StartTimer(startMovingTimerHandle, delayToStartMoving, () => StartMovingToFloor(GetNextFloor()));
    }

    private void RemoveMovingToFloorTimer()
    {
        TimerManager.RemoveTimer(startMovingTimerHandle);
    }

    //private void ResetMovingToFloorTimer()
    //{
    //    TimerManager.ResetTimer(startMovingTimerHandle);
    //}

    private void StopMoving()
    {
        Debug.Log("ElevatorStopMoving");
        isMoving = false;

        // Correct position.
        transform.position = new Vector3(transform.position.x, floorIndex * CityManager.floorHeight + CityManager.firstFloorHeight, transform.position.z);

        // Stop entities riding.
        foreach (Entity riders in ridingPassengers.ToArray()) {
            riders.OnElevatorPlatformStopped(this);
        }
        foreach (Entity waiters in waitingPassengers.ToArray()) {
            if (ridingPassengers.Count + goingToRidingPassengers.Count >= ownedBuilding.currentLevelData.maxResidentsCount)
                break;
            waiters.OnElevatorPlatformStopped(this);
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
            foreach (Entity rider in ridingPassengers) {
                if (rider.CurrentPathBuilding) {
                    nextFloorIndex = rider.CurrentPathBuilding.floorIndex;
                    break;
                }
            }

            if (ridingPassengers.Count < ownedBuilding.currentLevelData.maxResidentsCount && waitingPassengers.Count > 0) {
                foreach (Entity waiter in waitingPassengers) {
                    if (nextFloorIndex < floorIndex && waiter.floorIndex < floorIndex) {
                        nextFloorIndex = math.max(nextFloorIndex, waiter.floorIndex);
                    }
                    else if (nextFloorIndex > floorIndex && waiter.floorIndex > floorIndex) {
                        nextFloorIndex = math.min(nextFloorIndex, waiter.floorIndex);
                    }
                }
            }
            else {
                foreach (Entity rider in ridingPassengers) {
                    int pathFloor = rider.CurrentPathBuilding.floorIndex;
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
            ridingPassengers[i].Move(direction, speed);
    }

    public void AddPassenger(Entity passenger)
    {
        switch (passenger.elevatorPassengerState) {
            case ElevatorPassengerState.GoingToWaiting:
                goingForWaitingPassengers.Add(passenger);
                break;
            case ElevatorPassengerState.Waiting:
                waitingPassengers.Add(passenger);
                if (isMoving)
                    StartMovingToFloor(GetNextFloor());
                else
                    StartMovingToFloorTimer();
                break;
            case ElevatorPassengerState.GoingToRiding:
                goingToRidingPassengers.Add(passenger);
                RemoveMovingToFloorTimer();
                break;
            case ElevatorPassengerState.Riding:
                ridingPassengers.Add(passenger);
                StartMovingToFloorTimer();
                break;

        }
    }

    public void RemovePassenger(Entity passenger)
    {
        switch (passenger.elevatorPassengerState) {
            case ElevatorPassengerState.GoingToWaiting:
                goingForWaitingPassengers.Remove(passenger);
                break;
            case ElevatorPassengerState.Waiting:
                waitingPassengers.Remove(passenger);
                break;
            case ElevatorPassengerState.GoingToRiding:
                goingToRidingPassengers.Remove(passenger);
                break;
            case ElevatorPassengerState.Riding:
                ridingPassengers.Remove(passenger);
                if (ridingPassengers.Count > 0)
                    TimerManager.ResetTimer(startMovingTimerHandle);
                else
                    TimerManager.RemoveTimer(startMovingTimerHandle);
                break;

        }
    }

    private void OnEntityStopped(Entity entity)
    {
        Debug.Log(entity.elevatorPassengerState);

        if (entity.IsRidingOnElevator && entity.CurrentElevator == ownedElevator) {
            
        }
        else if (entity.IsWaitingForElevator) {
            StartMovingToFloor(GetNextFloor());
        }
    }

    public void SetOwnedBuilding(int newFloorIndex)
    {
        if (newFloorIndex != floorIndex && newFloorIndex >= 0) {
            ownedBuilding = cityManager.builtFloors[newFloorIndex].roomBuildingPlaces[placeIndex].placedBuilding;
            foreach (Entity npc in ridingPassengers) {
                npc.OnElevatorCabinChangedFloor(this);
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
