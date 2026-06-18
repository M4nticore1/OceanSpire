using System;
using UnityEngine;

public enum ElevatorPassengerStateEnum
{
    None,
    GoingToWaiting,
    Waiting,
    GoingToRiding,
    Riding,
    Exiting
}

public class ElevatorPassenger : MonoBehaviour
{
    [SerializeField] private CreatureCityNavigator cityNavigator;
    public CreatureCityNavigator CityNavigator => cityNavigator;

    public ElevatorModule CurrentElevator { get; private set; }

    [field: SerializeField] public ElevatorPassengerStateEnum CurrentStateEnum { get; private set; } = ElevatorPassengerStateEnum.None;
    public ElevatorPassengerState CurrentState { get; private set; }

    public bool IsGoingToWaiting => CurrentStateEnum == ElevatorPassengerStateEnum.GoingToWaiting;
    public bool IsWaiting => CurrentStateEnum == ElevatorPassengerStateEnum.Waiting;
    public bool IsGoingToRiding => CurrentStateEnum == ElevatorPassengerStateEnum.GoingToRiding;
    public bool IsRiding => CurrentStateEnum == ElevatorPassengerStateEnum.Riding;

    private void OnEnable()
    {
        cityNavigator.OnEnteredBuilding += OnEnteredBuilding;
        cityNavigator.OnExitedBuilding += OnExitedBuilding;
    }

    private void OnDisable()
    {
        cityNavigator.OnEnteredBuilding -= OnEnteredBuilding;
        cityNavigator.OnExitedBuilding -= OnExitedBuilding;
    }

    public void Init(ElevatorPassengerData elevatorPassengerData)
    {
        if (elevatorPassengerData == null) {
            Debug.LogError("elevatorPassengerData is not valid", this);
            return;
        }

        SetState(elevatorPassengerData.PassengerState);
    }

    public void SetState(ElevatorPassengerStateEnum state)
    {
        CurrentStateEnum = state;

        if (CurrentState != null) {
            CurrentState.Exit();
        }

        switch (state) {
            case ElevatorPassengerStateEnum.None:
                CurrentState = new NoneElevatorPassengerState(this);
                break;
            case ElevatorPassengerStateEnum.GoingToWaiting:
                CurrentState = new GoingToWatingElevatorPassengerState(this);
                break;
            case ElevatorPassengerStateEnum.Waiting:
                CurrentState = new WaitingElevatorPassengerState(this);
                break;
            case ElevatorPassengerStateEnum.GoingToRiding:
                CurrentState = new GoingToRidingElevatorPassengerState(this);
                break;
            case ElevatorPassengerStateEnum.Riding:
                CurrentState = new RidingElevatorPassengerState(this);
                break;
            case ElevatorPassengerStateEnum.Exiting:
                CurrentState = new ExitingElevatorPathState(this);
                break;
        }

        CurrentState.Enter();
    }

    public void OnElevatorChangedFloor(Building building)
    {
        cityNavigator.EnterBuilding(building);
    }

    public void OnElevatorStopped()
    {
        cityNavigator.FollowPath();
    }

    private void UpdateCurrentElevator()
    {
        var currentBuilding = cityNavigator.CurrentBuilding;
        if (currentBuilding) {
            CurrentElevator = currentBuilding.GetComponent<ElevatorModule>();
        }
        else {
            CurrentElevator = null;
        }
    }

    private void OnEnteredBuilding(Building building)
    {
        UpdateCurrentElevator();
    }

    private void OnExitedBuilding(Building building)
    {
        UpdateCurrentElevator();
    }
}