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

    [SerializeField] private HealthComponent healthComponent;
    public HealthComponent HealthComponent => healthComponent;

    [SerializeField] private int priority = 0;
    public int Priority => priority;

    [field: SerializeField] public ElevatorPassengerStateEnum CurrentStateEnum { get; private set; } = ElevatorPassengerStateEnum.None;
    public ElevatorPassengerState CurrentState { get; private set; }

    public bool IsGoingToWaiting => CurrentStateEnum == ElevatorPassengerStateEnum.GoingToWaiting;
    public bool IsWaiting => CurrentStateEnum == ElevatorPassengerStateEnum.Waiting;
    public bool IsGoingToRiding => CurrentStateEnum == ElevatorPassengerStateEnum.GoingToRiding;
    public bool IsRiding => CurrentStateEnum == ElevatorPassengerStateEnum.Riding;

    private void OnEnable()
    {
        healthComponent.OnDied += OnDied;
    }

    private void OnDisable()
    {
        healthComponent.OnDied -= OnDied;
    }

    public void Init()
    {
        Init(ElevatorPassengerData.Default());
    }

    public void Init(ElevatorPassengerData elevatorPassengerData)
    {
        if (elevatorPassengerData == null) {
            Debug.LogError($"[{nameof(ElevatorPassenger)}] ElevatorPassengerData is not valid", this);
            Init();
            return;
        }

        if (cityNavigator.EnteredElevator) {
            SetState(elevatorPassengerData.State);
        }
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
        cityNavigator.RunUpdateFollowingPathEndOfFrame();
    }

    private void OnDied()
    {
        SetState(ElevatorPassengerStateEnum.None);
    }
}