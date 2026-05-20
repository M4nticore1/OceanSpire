using UnityEngine;

public class BoatMovingToDockState : BoatState
{
    public BoatMovingToDockState(Boat boat) : base(boat)
    {
        
    }

    public override void Enter()
    {
        boat.Movement.TryMoveTo(boat.DockPoint.DockTransform.position);
    }

    public override void Exit()
    {
    }

    public override void Tick()
    {
        boat.ProcessDrainHealth();
    }

    public override void OnReachedPath()
    {
        boat.OnReturnedToDock();
    }
}