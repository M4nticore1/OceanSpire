using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElevatorCabinConstruction : BuildingConstruction
{
    public ElevatorModule OwnedElevator => OwnedBuilding.GetComponent<ElevatorModule>();

    public int FloorIndex = 0;
    public int PlaceIndex = 0;

    [SerializeField] private List<ElevatorPassenger> goingForWaitingPassengers = new();
    public IReadOnlyList<ElevatorPassenger> GoingForWaitingPassengers => goingForWaitingPassengers;

    [SerializeField] private List<ElevatorPassenger> waitingPassengers = new();
    public IReadOnlyList<ElevatorPassenger> WaitingPassengers => waitingPassengers;

    [SerializeField] private List<ElevatorPassenger> goingToRidingPassengers = new();
    public IReadOnlyList<ElevatorPassenger> GoingToRidingPassengers => goingToRidingPassengers;

    [SerializeField] private List<ElevatorPassenger> ridingPassengers = new();
    public IReadOnlyList<ElevatorPassenger> RidingPassengers => ridingPassengers;

    public bool IsMoving = false;
    public int StartFloorIndex = 0;
    public int TargetFloor = 0;
    public int NextFloor = 0;

    private float moveSpeed = 0f;
    private Vector3 moveDirection = Vector3.zero;

    private TimerHandle startMovingTimerHandle = new TimerHandle();
    private const float delayToStartMoving = 1f;

    public event Action OnMovementStarted;
    public event Action OnMovementStopped;
    
    public static event Action<ElevatorCabinConstruction> OnElevatorCabinStopped;
    public static event Action<ElevatorCabinConstruction> OnElevatorCabinChangedFloor;

    private void Update()
    {
        if (!IsMoving) return;

        float speed = moveSpeed * Time.deltaTime;
        Move(moveDirection, speed);

        int floor = GetFloorIndexByPosition();
        if (TryApplyOwnedBuildingByFloor(floor)) {
            if (TryStopMoving()) {
                ApplyConstructionPosition();
                SetTargetFloor(CalculateTargetFloor());
                UpdateMoveDirection();
                StartMovingToTargetFloorTimer();
            }
        }
    }

    protected override void OnInited(BuildingConstructionData data)
    {
        base.OnInited(data);

        var elevatorCabinData = data as ElevatorCabinData;
        if (elevatorCabinData == null) return;

        transform.position = new Vector3(transform.position.x, elevatorCabinData.Height, transform.position.z);
        SetTargetFloor(elevatorCabinData.TargetFloor);
        SetNextFloor(CalculateNextFloor());
        TryMoveToFloor(TargetFloor);
    }

    public override void SetOwnedBuilding(Building building)
    {
        base.SetOwnedBuilding(building);

        if (building is not TowerBuilding towerBuilding) {
            Debug.Log($"Tower Building not found at {name}");
            return;
        }

        SetFloorIndex(towerBuilding.FloorIndex);
        SetPlaceIndex(towerBuilding.PlaceIndex);

        NotifyPassengersAboutFloorChange();
    }

    public void SetTargetFloor(int floorIndex)
    {
        TargetFloor = floorIndex;

        SetNextFloor(CalculateNextFloor());
    }

    public void SetNextFloor(int floorIndex)
    {
        NextFloor = floorIndex;
    }

    public void StopMoving()
    {
        SetIsMoving(false);

        // Stop entities riding
        foreach (var rider in ridingPassengers.ToArray()) {
            rider.OnElevatorStopped();
        }
        foreach (var waiter in waitingPassengers.ToArray()) {
            waiter.OnElevatorStopped();
        }

        OnMovementStopped?.Invoke();
    }

    // Waiting Passengers
    public void AddGoingToWaitingPassenger(ElevatorPassenger elevatorPassenger)
    {
        goingForWaitingPassengers.Add(elevatorPassenger);
        AssignInteract(elevatorPassenger.CityNavigator);
    }

    public void RemoveGoingToWaitingPassenger(ElevatorPassenger elevatorPassenger)
    {
        goingForWaitingPassengers.Remove(elevatorPassenger);
        RemoveInteract(elevatorPassenger.CityNavigator);
    }

    // Waiting Passengers
    public void AddWaitingPassenger(ElevatorPassenger elevatorPassenger)
    {
        waitingPassengers.Add(elevatorPassenger);
        AssignInteract(elevatorPassenger.CityNavigator);

        UpdateDestinationAndProceed();
    }

    public void RemoveWaitingPassenger(ElevatorPassenger elevatorPassenger)
    {
        waitingPassengers.Remove(elevatorPassenger);
        RemoveInteract(elevatorPassenger.CityNavigator);

        UpdateDestinationAndProceed();
    }

    // Going To Riding Passengers
    public void AddGoingToRidingPassenger(ElevatorPassenger elevatorPassenger)
    {
        goingToRidingPassengers.Add(elevatorPassenger);
        AssignInteract(elevatorPassenger.CityNavigator);

        UpdateDestinationAndProceed();
    }

    public void RemoveGoingToRidingPassenger(ElevatorPassenger elevatorPassenger)
    {
        goingToRidingPassengers.Remove(elevatorPassenger);
        RemoveInteract(elevatorPassenger.CityNavigator);

        UpdateDestinationAndProceed();
    }

    // Riding Passengers
    public void AddRidingPassenger(ElevatorPassenger elevatorPassenger)
    {
        ridingPassengers.Add(elevatorPassenger);
        AssignInteract(elevatorPassenger.CityNavigator);

        UpdateDestinationAndProceed();
    }

    public void RemoveRidingPassenger(ElevatorPassenger elevatorPassenger)
    {
        ridingPassengers.Remove(elevatorPassenger);
        RemoveInteract(elevatorPassenger.CityNavigator);

        UpdateDestinationAndProceed();
    }

    // Passengers
    public void UpdateWaitingPassengers()
    {
        for (int i = waitingPassengers.Count - 1; i >= 0; i--) {
            var passenger = waitingPassengers[i];
            int floor = passenger.CityNavigator.FloorIndex;

            if (!ShouldMoveToFloor(floor)) {
                RemoveWaitingPassenger(passenger);
            }
        }
    }

    public void UnloadRidingPassengers()
    {
        for (int i = ridingPassengers.Count - 1; i >= 0; i--) {
            var passenger = ridingPassengers[i];
            passenger.SetState(ElevatorPassengerStateEnum.GoingToWaiting);
        }
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // Floor
    public bool TryMoveToFloor(int floor)
    {
        if (!ShouldMoveToFloor(floor))
            return false;

        StartMovingToFloor(floor);

        return true;
    }

    public bool ShouldMoveToFloor(int floor)
    {
        if (FloorIndex == floor) return false;

        var targetBuilding = BuildingsManager.Instance.BuiltFloors[floor].RoomBuildingPlaces[PlaceIndex].PlacedBuilding;
        if (!targetBuilding) return false;

        if (!targetBuilding.NetworkWith(OwnedElevator.OwnedTowerBuilding)) return false;

        return true;
    }

    private void UpdateDestinationAndProceed()
    {
        if (IsMoving) {
            int targetFloor = 0;

            if (ridingPassengers.Count > 0) {
                targetFloor = CalculateTargetFloor();
            }
            else {
                targetFloor = NextFloor;
            }

            SetTargetFloor(targetFloor);
            UpdateMoveDirection();
            StartMovingToTargetFloor();
        }
        else {
            SetTargetFloor(CalculateTargetFloor());
            UpdateMoveDirection();
            StartMovingToTargetFloorTimer();
        }
    }

    private void StartMovingToTargetFloor()
    {
        StartMovingToFloor(TargetFloor);
    }

    private void StartMovingToFloor(int floorIndex)
    {
        if (floorIndex == FloorIndex) return;

        SetIsMoving(true);
        StartFloorIndex = FloorIndex;

        OnMovementStarted?.Invoke();
    }

    private void StartMovingToTargetFloorTimer()
    {
        if (TargetFloor == FloorIndex) return;

        TimerManager.Instance.StartTimer(startMovingTimerHandle, delayToStartMoving, StartMovingToTargetFloor);
    }

    private void UpdateMoveDirection()
    {
        if (TargetFloor > FloorIndex)
            moveDirection = Vector3.up;
        else if (TargetFloor < FloorIndex)
            moveDirection = Vector3.down;
        else
            moveDirection = Vector3.zero;
    }

    private void RemoveMovingToFloorTimer()
    {
        TimerManager.Instance.RemoveTimer(startMovingTimerHandle);
    }

    private void SetFloorIndex(int value)
    {
        FloorIndex = value;
    }

    private void SetPlaceIndex(int value)
    {
        PlaceIndex = value;
    }

    private bool TryStopMoving()
    {
        if (!IsMoving) return false;
        if (FloorIndex != TargetFloor/* && !ShouldMoveToFloor(TargetFloor)*/) return false;

        StopMoving();
        return true;
    }

    private void SetIsMoving(bool value)
    {
        IsMoving = value;
    }

    private void NotifyPassengersAboutFloorChange()
    {
        foreach (var npc in ridingPassengers.ToArray()) {
            npc.OnElevatorChangedFloor(OwnedBuilding);
        }
    }

    private int CalculateTargetFloor()
    {
        if (goingToRidingPassengers.Count > 0)
            return goingToRidingPassengers[0].CityNavigator.FloorIndex;

        int currentFloor = FloorIndex;
        int freeSpace = OwnedBuilding.LevelDefinition.MaxHumansCount - ridingPassengers.Count;
        var possibleFloors = new List<int>();

        possibleFloors.AddRange(ridingPassengers
            .Where(p => p.CityNavigator.CurrentPathTowerBuilding)
            .Select(p => p.CityNavigator.CurrentPathTowerBuilding.FloorIndex));

        if (freeSpace > 0) {
            possibleFloors.AddRange(waitingPassengers
                .Where(p => p.CityNavigator.FloorIndex != currentFloor)
                .Select(p => p.CityNavigator.FloorIndex));
        }

        if (possibleFloors.Count == 0)
            return currentFloor;

        return possibleFloors
            .OrderBy(floor => Mathf.Abs(floor - currentFloor))
            .First();
    }

    private int CalculateNextFloor()
    {
        return TargetFloor > FloorIndex ? FloorIndex + 1 : TargetFloor < FloorIndex ? FloorIndex - 1 : FloorIndex;
    }

    private void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }

    private int GetFloorIndexByPosition()
    {
        int floorIndex = FloorIndex;
        float firstFloorHeight = BuildingsManager.FirstFloorHeight;
        float floorHeight = BuildingsManager.FloorHeight;

        if (TargetFloor > FloorIndex) {
            floorIndex = (int)((transform.position.y - firstFloorHeight) / floorHeight);
            if (floorIndex < StartFloorIndex)
                floorIndex = StartFloorIndex;
        }
        else if (TargetFloor < FloorIndex) {
            floorIndex = (int)((transform.position.y - firstFloorHeight + floorHeight) / floorHeight);
            if (floorIndex > StartFloorIndex)
                floorIndex = StartFloorIndex;
        }

        return floorIndex;
    }

    private bool TryApplyOwnedBuildingByFloor(int floor)
    {
        var building = BuildingsManager.Instance.BuiltFloors[floor].RoomBuildingPlaces[PlaceIndex].PlacedBuilding;
        if (building == OwnedBuilding) return false;

        SetOwnedBuilding(building);
        return true;
    }
}