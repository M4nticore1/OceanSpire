using UnityEngine;

public class RidingElevatorPassengerState : ElevatorPassengerState
{
    public RidingElevatorPassengerState(ElevatorPassenger elevatorPassenger) : base(elevatorPassenger)
    {

    }

    public override void Enter()
    {
        var cityNavigator = elevatorPassenger.CityNavigator;
        var currentElevator = elevatorPassenger.CurrentElevator;

        if (!currentElevator) {
            Debug.LogError("currentElevator is not valid");
            return;
        }

        currentElevator.AddRidingPassenger(elevatorPassenger);
        cityNavigator.Movement.SetAgentEnabled(false);
        cityNavigator.transform.SetParent(currentElevator.SpawnedElevatorCabin.transform);
    }

    public override void Exit()
    {
        var cityNavigator = elevatorPassenger.CityNavigator;
        var currentElevator = elevatorPassenger.CurrentElevator;

        if (!currentElevator) {
            Debug.LogError("currentElevator is not valid");
            return;
        }

        cityNavigator.Movement.SetAgentEnabled(true);
        cityNavigator.transform.SetParent(null);
        currentElevator.RemoveRidingPassenger(elevatorPassenger);
    }
}