using UnityEngine;

public class CollectingLootBoatState : BoatState, IProgressable
{
    private float currentCollectingTime = 0f;
    private float collectLootTime = 2f;

    public CollectingLootBoatState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        if (!boat.TargetDriftingLoot) {
            boat.SetTargetLoot(DriftingLootFinder.TryFindNearestSwimmingDriftingLoot(DriftingLootManager.Instance, boat.transform.position));
        }

        if (!TryStopDriftingLoot()) {
            boat.SetState(BoatStateEnum.FindingLoot);
        }
    }

    public override void Exit()
    {
        var container = boat.TargetDriftingLoot;
        if (!container) return;

        container.StartMoving();
    }

    public override void Tick()
    {
        TryStopDriftingLoot();

        currentCollectingTime += Time.deltaTime;
        if (currentCollectingTime <= collectLootTime) return;

        TryCollectLoot();
        UpdateState();
    }

    public override void OnReachedPath()
    {

    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {
        
    }

    public float GetProgress()
    {
        return currentCollectingTime / collectLootTime;
    }

    private void UpdateState()
    {
        if (boat.Inventory.RemainingWeight > 0) {
            boat.SetState(BoatStateEnum.FindingLoot);
        }
        else {
            boat.SetState(BoatStateEnum.MovingToDock);
        }
    }

    private bool TryStopDriftingLoot()
    {
        if (!boat) return false;
        if (!boat.TargetDriftingLoot) return false;

        boat.TargetDriftingLoot.StopMoving();
        return true;
    }

    private bool TryCollectLoot()
    {
        if (!ShouldCollectLoot()) return false;

        var collectedLoot = boat.TargetDriftingLoot.TakeItems();

        foreach (var loot in collectedLoot) {
            if (boat.Inventory.RemainingWeight <= 0f) break;

            boat.Inventory.AddItem(loot.Definition.ItemId, loot.Amount);
        }

        return true;
    }

    private bool ShouldCollectLoot()
    {
        if (!boat) return false;
        if (!boat.TargetDriftingLoot) return false;

        return true;
    }
}