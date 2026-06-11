using UnityEngine;

public class BoatMovingToLootState : BoatFindingLootState
{
    public BoatMovingToLootState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        base.Enter();

        TryStartMovingToTarget();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Tick()
    {
        base.Tick();

        TryStartMovingToTarget();
    }

    public override void OnReachedPath()
    {
        base.OnReachedPath();

        boat.SetState(BoatStateEnum.CollectingLoot);
    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {

    }

    private void TryStartMovingToTarget()
    {
        if (!ShouldStartMovingToTarget()) return;

        boat.Movement.TryMoveTo(boat.TargetDriftingLoot.transform.position);
    }

    private bool ShouldStartMovingToTarget()
    {
        if (!boat) return false;
        if (!boat.TargetDriftingLoot) return false;

        return true;
    }
}