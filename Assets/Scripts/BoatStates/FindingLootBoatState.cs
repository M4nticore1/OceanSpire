using UnityEngine;

public class FindingLootBoatState : BoatState
{
    private const double updateDestinationRate = 0.5f;
    private double lastUpdateDestinationTime = 0;

    public DriftingLoot currentTarget { get; private set; } = null;
    public bool isCollectingLoot { get; private set; } = false;

    DriftingLootFocusManager focusedLootManager = DriftingLootFocusManager.Instance;

    public FindingLootBoatState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        boat.Movement.StopMoving();
        boat.RemoveTargetLoot();
        TryUpdateTarget();
    }

    public override void Exit()
    {

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
        var nearestFocusedLoot = focusedLootManager ? focusedLootManager.GetNearestAvaliableFocusedDriftingLoot(boat) : null;
        if (nearestFocusedLoot && boat.TrySetTargetLoot(nearestFocusedLoot)) {
            return;
        }

        var nearestLoot = DriftingLootFinder.TryFindNearestSwimmingDriftingLoot(DriftingLootManager.Instance, boat);
        boat.TrySetTargetLoot(nearestLoot);
    }
}