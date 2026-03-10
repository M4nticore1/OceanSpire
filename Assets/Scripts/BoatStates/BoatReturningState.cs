using UnityEngine;

public class BoatReturningState : BoatState
{
    public BoatReturningState(Boat boat) : base(boat)
    {
        
    }

    public override void Enter()
    {
        StartMovingToDock();
    }

    public override void Exit()
    {
    }

    public override void Process()
    {
        boat.ProcessDrainHealth();
    }

    public override void HandleReachedPath()
    {
        boat.HandleReturnedToDock();
    }

    private void StartMovingToDock()
    {
        boat.Movement.MoveTo(boat.dockPoint.DockTransform.position);
    }
}