using System.Data;
using UnityEngine;

public class FindingLootBoatState : BoatState
{
    private const float updateDestinationRate = 0.5f;
    private float currentUpdateDestinationTime = 0;

    public DriftingLoot currentTarget { get; private set; } = null;
    public bool isCollectingLoot { get; private set; } = false;

    DriftingLootFocusManager focusedLootManager = DriftingLootFocusManager.Instance;

    public FindingLootBoatState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        if (boat.CurrentRider == null) {
            boat.SetState(BoatStateEnum.MovingToDock);
            return;
        }

        boat.Movement.TryStopMoving();
        boat.RemoveTargetLoot();

        TryStopExitingBoat();
        UpdateTarget();
    }

    public override void Exit()
    {

    }

    public override void Tick()
    {
        currentUpdateDestinationTime += Time.deltaTime;
        if (currentUpdateDestinationTime >= updateDestinationRate) {
            UpdateTarget();
            UpdateState();
            currentUpdateDestinationTime = 0f;
        }
    }

    public override void OnReachedPath()
    {
        
    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {

    }

    private void UpdateTarget()
    {
        var nearestFocusedLoot = focusedLootManager?.GetNearestAvailableFocusedDriftingLoot(boat);
        if (nearestFocusedLoot != null) {
            if (boat.TrySetTargetLoot(nearestFocusedLoot))
                return;
        }

        var nearestLoot = DriftingLootFinder.TryFindNearestSwimmingDriftingLoot(DriftingLootManager.Instance, boat);
        if (nearestLoot != null) {
            boat.TrySetTargetLoot(nearestLoot);
        }
    }

    private void UpdateState()
    {
        if (boat.TargetDriftingLoot != null) {
            boat.SetState(BoatStateEnum.MovingToLoot);
        }
    }

    private void TryStopExitingBoat()
    {
        boat.CurrentRider.StopExitingBoat();
    }
}