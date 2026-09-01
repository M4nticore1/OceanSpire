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
            UpdateState();
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
        UpdateState();
    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {
        UpdateMovement();
    }

    private void UpdateState()
    {
        if (boat.Inventory.GetCurrentWeight() > 0) {
            boat.SetState(BoatStateEnum.UnloadingLoot);
        }
        else {
            boat.SetState(BoatStateEnum.Idle);
        }
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
            boat.Movement.TryStopMoving();
        }
    }

    private bool IsReachedDock()
    {
        if (!boat.DockPoint) return false;
        if (!boat.Movement.IsReachedPosition(boat.DockPoint.DockTransform.position)) return false;

        return true;
    }
}