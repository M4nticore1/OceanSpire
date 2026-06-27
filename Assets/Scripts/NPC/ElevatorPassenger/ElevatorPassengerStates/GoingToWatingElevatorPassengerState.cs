using UnityEngine;

public class GoingToWatingElevatorPassengerState : ElevatorPassengerState
{
    public GoingToWatingElevatorPassengerState(ElevatorPassenger elevatorPassenger) : base(elevatorPassenger)
    {

    }

    public override void Enter()
    {
        var cityNavigator = elevatorPassenger.CityNavigator;

        var currentBuilding = cityNavigator.CurrentBuilding;
        if (!currentBuilding) {
            Debug.LogError("currentBuilding is not valid");
            return;
        }

        var currentElevator = cityNavigator.CurrentElevator;
        if (!currentElevator) {
            Debug.LogError("currentElevator is not valid");
            return;
        }

        var construction = currentBuilding.SpawnedConstruction;
        if (!construction) {
            Debug.LogError("construction is not valid");
            return;
        }

        construction.AssignInteract(cityNavigator);
        currentElevator.AddGoingToWaitingPassenger(elevatorPassenger);
        cityNavigator.Movement.TryMoveTo(construction.GetInteraction(cityNavigator).GetWaypoint(0).Transform);
    }

    public override void Exit()
    {
        var cityNavigator = elevatorPassenger.CityNavigator;

        var currentBuilding = cityNavigator.CurrentBuilding;
        if (!currentBuilding) {
            Debug.LogError("currentBuilding is not valid");
            return;
        }

        var currentElevator = cityNavigator.CurrentElevator;
        if (!currentElevator) {
            Debug.LogError("currentElevator is not valid");
            return;
        }

        var construction = currentBuilding.SpawnedConstruction;
        if (!construction) {
            Debug.LogError("construction is not valid");
            return;
        }

        construction.RemoveInteract(cityNavigator);
        currentElevator.RemoveGoingToWaitingPassenger(elevatorPassenger);
    }
}