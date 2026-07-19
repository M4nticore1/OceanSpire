using UnityEngine;

public class MovingToDockBoatState : BoatState
{
    public MovingToDockBoatState(Boat boat) : base(boat)
    {
        
    }

    public override void Enter()
    {
        boat.RemoveTargetLoot();

        if (IsReachedDock()) {
            boat.OnReturnedToDock();
            return;
        }

        UpdateMovement();
    }

    public override void Exit()
    {

    }

    public override void Tick()
    {

    }

    public override void OnReachedPath()
    {
        boat.OnReturnedToDock();
    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        if (boat.DockPoint) {
            if (!boat.Movement.TryMoveTo(boat.DockPoint.DockTransform.position)) {
                Debug.LogError($"[{nameof(MovingToDockBoatState)}] Boat can't move to its dock!");
            }
        }
        else {
            Debug.LogError($"[{nameof(MovingToDockBoatState)}] Boat Dock is not valid!");
            boat.Movement.StopMoving();
        }
    }

    private bool IsReachedDock()
    {
        if (!boat.DockPoint) return false;
        if (!boat.Movement.IsReachedPosition(boat.DockPoint.DockTransform.position)) return false;

        return true;
    }
}