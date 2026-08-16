using UnityEngine;

public abstract class ElevatorPassengerState
{
    public ElevatorPassenger ElevatorPassenger { get; private set; }
    public ElevatorModule EnteredElevator { get; private set; }
    public CreatureCityNavigator CityNavigator => ElevatorPassenger.CityNavigator;

    public ElevatorPassengerState(ElevatorPassenger elevatorPassenger)
    {
        if (elevatorPassenger == null) {
            Debug.LogError($"[{nameof(ElevatorPassengerState)}] Elevator Passenger is not valid!");
            return;
        }

        ElevatorPassenger = elevatorPassenger;

        EnteredElevator = elevatorPassenger.CityNavigator.EnteredElevator;
        if (EnteredElevator == null && this is not NoneElevatorPassengerState) {
            Debug.LogError($"[{nameof(ElevatorPassengerState)}] Entered Elevator is not valid!");
        }
    }

    public abstract void Enter();
    public abstract void Exit();
}