using UnityEngine;

public class BoatMovingToDockState : BoatState
{
    public BoatMovingToDockState(Boat boat) : base(boat)
    {
        
    }

    public override void Enter()
    {
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
            boat.Movement.TryMoveTo(boat.DockPoint.DockTransform.position);
        }
        else {
            boat.Movement.TryStopMoving();
        }
    }
}