using UnityEngine;

public class MovingToLootBoatState : FindingLootBoatState
{
    private float updateDestinationFrequency = 0.5f;
    private float updateDestinationTime = 0.0f;

    public MovingToLootBoatState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        base.Enter();

        if (!boat.TargetDriftingLoot) {
            boat.SetState(BoatStateEnum.FindingLoot);
            return;
        }

        TryStartMovingToTarget();
    }

    public override void Tick()
    {
        base.Tick();

        updateDestinationTime += Time.deltaTime;

        if (updateDestinationTime > updateDestinationFrequency) {
            TryStartMovingToTarget();
            updateDestinationTime = 0;
        }
    }

    public override void OnReachedPath()
    {
        base.OnReachedPath();

        if (boat.TargetDriftingLoot == null) {
            boat.SetState(BoatStateEnum.FindingLoot);
            return;
        }

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
        if (boat == null) return false;
        if (!boat.TargetDriftingLoot) return false;

        return true;
    }
}