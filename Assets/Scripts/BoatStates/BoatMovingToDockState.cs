using UnityEngine;

public class BoatMovingToDockState : BoatState
{
    public BoatMovingToDockState(Boat boat) : base(boat)
    {
        
    }

    public override void Enter()
    {
        StartMovingToDock();
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

    private void StartMovingToDock()
    {
        boat.Movement.TryMoveTo(boat.DockPoint.DockTransform.position);
    }
}