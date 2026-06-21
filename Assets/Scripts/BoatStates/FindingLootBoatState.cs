using UnityEngine;

public class FindingLootBoatState : BoatState
{
    private const double updateDestinationRate = 0.5f;
    private double lastUpdateDestinationTime = 0;

    public DriftingLoot currentTarget { get; private set; } = null;
    public bool isCollectingLoot { get; private set; } = false;

    public FindingLootBoatState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {

    }

    public override void Exit()
    {
        boat.SetTargetLoot(null);
    }

    public override void Tick()
    {
        if (Time.timeAsDouble < lastUpdateDestinationTime + updateDestinationRate) return;

        TryUpdateTarget();
        lastUpdateDestinationTime = Time.timeAsDouble;
    }

    public override void OnReachedPath()
    {
        
    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {

    }

    private void TryUpdateTarget()
    {
        var target = DriftingLootFinder.TryFindNearestSwimmingDriftingLoot(DriftingLootManager.Instance, boat.transform.position);

        TrySetTarget(target);
    }

    private void TrySetTarget(SwimmingDriftingLoot driftingLoot)
    {
        if (!ShouldSetTarget(driftingLoot)) return;

        boat.SetTargetLoot(driftingLoot);
        boat.SetState(BoatStateEnum.MovingToLoot);
    }

    private bool ShouldSetTarget(SwimmingDriftingLoot driftingLoot)
    {
        if (!driftingLoot) return false;

        return true;
    }
}