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

        if (!TryUpdateState()) {
            TryStartMovingToTarget();
        }
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

        if (!TryUpdateState()) {
            boat.SetState(BoatStateEnum.CollectingLoot);
        }
    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {

    }

    private void TryStartMovingToTarget()
    {
        if (!ShouldStartMovingToTarget()) return;

        boat.Movement.TryMoveTo(boat.TargetDriftingLoot.transform.position);
    }

    private bool TryUpdateState()
    {
        if (boat.TargetDriftingLoot == null) {
            boat.SetState(BoatStateEnum.FindingLoot);
            return true;
        }

        return false;
    }

    private bool ShouldStartMovingToTarget()
    {
        if (boat == null) return false;
        if (!boat.TargetDriftingLoot) return false;

        return true;
    }
}