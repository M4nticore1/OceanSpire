using UnityEngine;

public class ExitingElevatorPathState : ElevatorPassengerState
{
    public ExitingElevatorPathState(ElevatorPassenger elevatorPassenger) : base(elevatorPassenger)
    {

    }

    public override void Enter()
    {
        var cityNavigator = ElevatorPassenger.CityNavigator;
        var currentBuilding = cityNavigator.CurrentBuilding;

        if (currentBuilding == null) {
            Debug.LogError("currentBuilding is not valid");
            return;
        }

        var construction = currentBuilding.SpawnedConstruction;
        if (construction == null) {
            Debug.LogError("construction is not valid");
            return;
        }

        construction.InteractionPointsHandler.AssignInteractor(cityNavigator);
        cityNavigator.Movement.TryMoveTo(construction.InteractionPointsHandler.GetInteractPoint(cityNavigator).GetWaypoint(0).Transform);
    }

    public override void Exit()
    {
        var cityNavigator = ElevatorPassenger.CityNavigator;
        var currentBuilding = cityNavigator.CurrentBuilding;

        if (currentBuilding == null) return;

        var construction = currentBuilding.SpawnedConstruction;
        if (construction == null) {
            Debug.LogError("construction is not valid");
            return;
        }

        construction.InteractionPointsHandler.RemoveInteractor(cityNavigator);
    }
}