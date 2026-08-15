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

        this.ElevatorPassenger = elevatorPassenger;
        EnteredElevator = elevatorPassenger.CityNavigator.CurrentElevator;
    }

    public abstract void Enter();
    public abstract void Exit();
}