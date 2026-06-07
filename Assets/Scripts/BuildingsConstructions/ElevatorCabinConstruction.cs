using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ElevatorCabinConstruction : BuildingConstruction
{
    public int FloorIndex = 0;
    public int PlaceIndex = 0;

    [SerializeField] private List<CreatureCityNavigator> goingForWaitingPassengers = new();
    public IReadOnlyList<CreatureCityNavigator> GoingForWaitingPassengers => goingForWaitingPassengers;

    [SerializeField] private List<CreatureCityNavigator> waitingPassengers = new();
    public IReadOnlyList<CreatureCityNavigator> WaitingPassengers => waitingPassengers;

    [SerializeField] private List<CreatureCityNavigator> goingToRidingPassengers = new();
    public IReadOnlyList<CreatureCityNavigator> GoingToRidingPassengers => goingToRidingPassengers;

    [SerializeField] private List<CreatureCityNavigator> ridingPassengers = new();
    public IReadOnlyList<CreatureCityNavigator> RidingPassengers => ridingPassengers;

    public bool IsMoving { get; private set; } = false;
    public int StartFloorIndex { get; private set; } = 0;
    public int TargetFloor = 0;
    public int NextFloor = 0;

    private float moveSpeed => ((OwnedBuilding.GetComponent<ElevatorModule>().LevelData) as ElevatorModuleLevelData).ElevatorMoveSpeed;
    private Vector3 moveDirection = Vector3.zero;

    private TimerHandle startMovingTimerHandle = new TimerHandle();
    private const float delayToStartMoving = 1f;

    public ElevatorModule OwnedElevator => OwnedBuilding.GetComponent<ElevatorModule>();
    public static event System.Action<ElevatorCabinConstruction> onElevatorPlatformStopped;
    public static event System.Action<ElevatorCabinConstruction> onElevatorPlatformChangedFloor;

    private void Update()
    {
        if (!IsMoving) return;

        float speed = moveSpeed * Time.deltaTime;
        Move(moveDirection, speed);

        int floor = GetFloorIndexByPosition();
        ApplyOwnedBuildingByFloor(floor);
        //TryStopMoving();
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
        TryStopMoving();
    }

    public override void SetOwnedBuilding(Building building)
    {
        base.SetOwnedBuilding(building);

        UpdateFloorAndPlaceIndexes(building);

        foreach (var npc in ridingPassengers.ToArray()) {
            npc.OnElevatorChangedFloor(building);
        }

        UpdateTargetFloor();
        TryStopMoving();
    }

    public void SetTargetFloor(int floorIndex)
    {
        TargetFloor = floorIndex;
    }

    public void SetNextFloor(int floorIndex)
    {
        NextFloor = floorIndex;
    }

    public void StopMoving()
    {
        SetIsMoving(false);
        ApplyBuildingPosition();

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
        if (IsMoving) {
            UpdateTargetFloor();
            StartMovingToFloor(TargetFloor);
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
            TimerManager.Instance.ResetTimer(startMovingTimerHandle);
        else
            TimerManager.Instance.RemoveTimer(startMovingTimerHandle);
    }

    // Passengers
    public void UpdateWaitingPassengers()
    {
        for (int i = waitingPassengers.Count - 1; i >= 0; i--) {
            CreatureCityNavigator navigator = waitingPassengers[i];
            int floor = navigator.FloorIndex;

            if (!ShouldMoveToFloor(floor)) {
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

    private void StartMovingToFloor(int floorIndex)
    {
        SetIsMoving(true);
        StartFloorIndex = FloorIndex;

        if (floorIndex > FloorIndex)
            moveDirection = Vector3.up;
        else if (floorIndex < FloorIndex)
            moveDirection = Vector3.down;
    }

    private void StartMovingToFloorTimer()
    {
        UpdateTargetFloor();
        TimerManager.Instance.StartTimer(startMovingTimerHandle, delayToStartMoving, () => StartMovingToFloor(TargetFloor));
    }

    private void RemoveMovingToFloorTimer()
    {
        TimerManager.Instance.RemoveTimer(startMovingTimerHandle);
    }

    private void UpdateFloorAndPlaceIndexes(Building building)
    {
        var towerBuilding = building as TowerBuilding;
        FloorIndex = towerBuilding.FloorIndex;
        PlaceIndex = towerBuilding.PlaceIndex;
    }

    private void UpdateTargetFloor()
    {
        SetTargetFloor(CalculateTargetFloor());
    }

    private void TryStopMoving()
    {
        if (!IsMoving) return;
        if (FloorIndex != TargetFloor && !ShouldMoveToFloor(TargetFloor)) return;

        StopMoving();
    }

    private void SetIsMoving(bool value)
    {
        IsMoving = value;
    }

    private int CalculateTargetFloor()
    {
        if (goingToRidingPassengers.Count > 0) {
            return FloorIndex;
        }

        int? targetFloor = null;
        int maxPassengersCount = OwnedBuilding.LevelData.MaxHumansCount;

        foreach (var passenger in ridingPassengers) {
            if (!passenger.CurrentPathTowerBuilding) continue;

            int passengerTargetFloor = passenger.CurrentPathTowerBuilding.FloorIndex;

            if (targetFloor == null) {
                targetFloor = passengerTargetFloor;
                continue;
            }

            bool nearUp = targetFloor.Value > FloorIndex && passengerTargetFloor > FloorIndex && passengerTargetFloor < targetFloor.Value;
            bool nearDown = targetFloor.Value < FloorIndex && passengerTargetFloor < FloorIndex && passengerTargetFloor > targetFloor.Value;

            if (nearUp || nearDown)
                targetFloor = passengerTargetFloor;
        }

        for (int i = 0; i < Mathf.Min(maxPassengersCount, waitingPassengers.Count - ridingPassengers.Count); i++) {
            var passenger = waitingPassengers[i];
            int passengerFloor = passenger.FloorIndex;

            if (targetFloor == null) {
                targetFloor = passengerFloor;
                continue;
            }

            bool nearUp = targetFloor.Value > FloorIndex && passengerFloor > FloorIndex && passengerFloor < targetFloor.Value;
            bool nearDown = targetFloor.Value < FloorIndex && passengerFloor < FloorIndex && passengerFloor > targetFloor.Value;

            if (nearUp || nearDown)
                targetFloor = passengerFloor;
        }

        return targetFloor != null ? targetFloor.Value : FloorIndex;
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
        int floorIndex = 0;

        if (TargetFloor >= this.FloorIndex) {
            floorIndex = (int)((transform.position.y - BuildingsManager.FirstFloorHeight) / BuildingsManager.FloorHeight);
            if (floorIndex < StartFloorIndex)
                floorIndex = StartFloorIndex;
        }
        else {
            floorIndex = (int)((transform.position.y - BuildingsManager.FirstFloorHeight + BuildingsManager.FloorHeight) / BuildingsManager.FloorHeight);
            if (floorIndex > StartFloorIndex)
                floorIndex = StartFloorIndex;
        }
        return floorIndex;
    }

    private void ApplyOwnedBuildingByFloor(int floor)
    {
        var building = BuildingsManager.Instance.BuiltFloors[floor].RoomBuildingPlaces[PlaceIndex].PlacedBuilding;
        if (building == OwnedBuilding) return;

        SetOwnedBuilding(building);
    }
}