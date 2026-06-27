using UnityEngine;

public class GoingToRidingElevatorPassengerState : ElevatorPassengerState
{
    public GoingToRidingElevatorPassengerState(ElevatorPassenger elevatorPassenger) : base(elevatorPassenger)
    {

    }

    public override void Enter()
    {
        var cityNavigator = elevatorPassenger.CityNavigator;
        var currentElevator = cityNavigator.CurrentElevator;

        if (!currentElevator) {
            Debug.LogError("cityNavigator.CurrentElevator is not valid");
            return;
        }

        currentElevator.AddGoingToRidingPassenger(elevatorPassenger);
        cityNavigator.Movement.TryMoveTo(currentElevator.GetCabinRidingTransform(elevatorPassenger));
    }

    public override void Exit()
    {
        var cityNavigator = elevatorPassenger.CityNavigator;
        var currentElevator = cityNavigator.CurrentElevator;

        if (!currentElevator) {
            Debug.LogError("cityNavigator.CurrentElevator is not valid");
            return;
        }

        currentElevator.RemoveGoingToRidingPassenger(elevatorPassenger);
    }
}