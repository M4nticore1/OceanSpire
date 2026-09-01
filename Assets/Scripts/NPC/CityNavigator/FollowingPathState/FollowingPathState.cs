using UnityEngine;

public class FollowingPathState : PathState
{
    public FollowingPathState(CreatureCityNavigator cityNavigator) : base(cityNavigator)
    {

    }

    public override void Enter()
    {
        var targetBuilding = cityNavigator.TargetBuilding;
        if (!targetBuilding) {
            Debug.LogError("targetBuilding is not valid");
            return;
        }

        var currentBuilding = cityNavigator.EnteredBuilding;
        if (currentBuilding == targetBuilding) {
            var construction = currentBuilding.SpawnedConstruction;
            if (!construction) {
                Debug.LogError("construction is not valid");
                return;
            }

            construction.InteractionPointsHandler.AssignInteractor(cityNavigator);
            cityNavigator.Movement.TryMoveTo(cityNavigator.WaypointsComponent.GetCurrentWaypoint().Transform);
        }
        else {
            if (!cityNavigator.CurrentPathBuilding) {
                Debug.LogError("cityNavigator.CurrentPathBuilding is not valid");
                return;
            }

            cityNavigator.Movement.TryMoveTo(cityNavigator.CurrentPathBuilding.transform.position);
        }
    }

    public override void Exit()
    {
        var currentBuilding = cityNavigator.EnteredBuilding;
        if (!currentBuilding) return;

        var construction = currentBuilding.SpawnedConstruction;
        if (!construction) {
            Debug.LogError("construction is not valid");
            return;
        }

        construction.InteractionPointsHandler.RunRemoveInteractorEndOfFrame(cityNavigator);
    }
}