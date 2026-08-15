using UnityEngine;

public class GoingToWatingElevatorPassengerState : ElevatorPassengerState
{
    public GoingToWatingElevatorPassengerState(ElevatorPassenger elevatorPassenger) : base(elevatorPassenger)
    {

    }

    public override void Enter()
    {
        if (EnteredElevator == null) return;
        if (CityNavigator == null) return;

        var currentBuilding = EnteredElevator.OwnedBuilding;
        if (currentBuilding == null) {
            Debug.LogError($"[{nameof(GoingToWatingElevatorPassengerState)}] Current Building is not valid!");
            return;
        }

        var construction = currentBuilding.SpawnedConstruction;
        if (construction == null) {
            Debug.LogError($"[{nameof(GoingToWatingElevatorPassengerState)}] Construction is not valid!");
            return;
        }

        construction.InteractionPointsHandler.AssignInteractor(CityNavigator);
        EnteredElevator.AddGoingToWaitingPassenger(ElevatorPassenger);
        CityNavigator.Movement.TryMoveTo(construction.InteractionPointsHandler.GetInteractPoint(CityNavigator).GetWaypoint(0).Transform);
    }

    public override void Exit()
    {
        if (EnteredElevator == null) return;
        if (CityNavigator == null) return;

        var currentBuilding = EnteredElevator.OwnedBuilding;
        if (currentBuilding == null) return;

        var construction = currentBuilding.SpawnedConstruction;
        if (construction == null) {
            Debug.LogError($"[{nameof(GoingToWatingElevatorPassengerState)}] Construction is not valid!");
            return;
        }

        construction.InteractionPointsHandler.RemoveInteractor(CityNavigator);
        EnteredElevator.RemoveGoingToWaitingPassenger(ElevatorPassenger);
    }
}