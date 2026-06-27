using UnityEngine;

public class WaitingElevatorPassengerState : ElevatorPassengerState
{
    public WaitingElevatorPassengerState(ElevatorPassenger elevatorPassenger) : base(elevatorPassenger)
    {

    }

    public override void Enter()
    {
        var cityNavigator = elevatorPassenger.CityNavigator;
        var currentElevator = cityNavigator.CurrentElevator;

        if (!currentElevator) {
            Debug.LogError("currentElevator is not valid");
            return;
        }

        currentElevator.AddWaitingPassenger(elevatorPassenger);
    }

    public override void Exit()
    {
        var cityNavigator = elevatorPassenger.CityNavigator;
        var currentElevator = cityNavigator.CurrentElevator;

        if (!currentElevator) {
            Debug.LogError("currentElevator is not valid");
            return;
        }

        currentElevator.RemoveWaitingPassenger(elevatorPassenger);
    }
}