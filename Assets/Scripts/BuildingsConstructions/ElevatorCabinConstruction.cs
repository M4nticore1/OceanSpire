using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
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

    public bool IsMoving = false;
    public int StartFloorIndex = 0;
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
        if (TryApplyOwnedBuildingByFloor(floor)) {
            if (TryStopMoving()) {
                ApplyConstructionPosition();
                SetTargetFloor(CalculateTargetFloor());
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
            SetTargetFloor(CalculateTargetFloor());
            StartMovingToFloor(TargetFloor);
        }
        else {
            SetTargetFloor(CalculateTargetFloor());
            StartMovingToTargetFloorTimer();
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

        SetTargetFloor(CalculateTargetFloor());
        StartMovingToTargetFloorTimer();
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

    private void StartMovingToTargetFloor()
    {
        StartMovingToFloor(TargetFloor);
    }

    private void StartMovingToFloor(int floorIndex)
    {
        if (floorIndex == FloorIndex) return;

        SetIsMoving(true);
        StartFloorIndex = FloorIndex;

        if (floorIndex > FloorIndex)
            moveDirection = Vector3.up;
        else if (floorIndex < FloorIndex)
            moveDirection = Vector3.down;
        else
            moveDirection = Vector3.zero;
    }

    private void StartMovingToTargetFloorTimer()
    {
        if (TargetFloor == FloorIndex) return;

        TimerManager.Instance.StartTimer(startMovingTimerHandle, delayToStartMoving, StartMovingToTargetFloor);
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
        if (FloorIndex != TargetFloor && !ShouldMoveToFloor(TargetFloor)) return false;

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
        if (goingToRidingPassengers.Count > 0) {
            return goingToRidingPassengers[0].FloorIndex;
        }

        int currentFloor = FloorIndex;
        int? targetFloor = null;
        int maxPassengersCount = OwnedBuilding.LevelData.MaxHumansCount;

        foreach (var passenger in ridingPassengers) {
            if (!passenger.CurrentPathTowerBuilding) continue;
            int floor = passenger.CurrentPathTowerBuilding.FloorIndex;

            if (targetFloor == null) {
                targetFloor = floor;
                continue;
            }

            bool isMovingUp = targetFloor.Value > currentFloor;
            if (isMovingUp) {
                if (floor > currentFloor && floor < targetFloor.Value)
                    targetFloor = floor;
            }
            else {
                if (floor < currentFloor && floor > targetFloor.Value)
                    targetFloor = floor;
            }
        }

        int freeSpace = maxPassengersCount - ridingPassengers.Count;
        if (freeSpace > 0) {
            var sortedWaiting = waitingPassengers.OrderBy(p => Mathf.Abs(p.FloorIndex - currentFloor)).Take(freeSpace);

            foreach (var passenger in sortedWaiting) {
                int floor = passenger.FloorIndex;

                if (targetFloor == null) {
                    targetFloor = floor;
                    continue;
                }

                bool isMovingUp = targetFloor.Value > currentFloor;
                if (isMovingUp) {
                    if (floor > currentFloor && floor < targetFloor.Value)
                        targetFloor = floor;
                }
                else {
                    if (floor < currentFloor && floor > targetFloor.Value)
                        targetFloor = floor;
                }
            }
        }

        return targetFloor ?? currentFloor;
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
        float firstFloorHeight = BuildingsManager.FirstFloorHeight;
        float floorHeight = BuildingsManager.FloorHeight;

        if (TargetFloor >= FloorIndex) {
            floorIndex = (int)((transform.position.y - firstFloorHeight) / floorHeight);
            if (floorIndex < StartFloorIndex)
                floorIndex = StartFloorIndex;
        }
        else {
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