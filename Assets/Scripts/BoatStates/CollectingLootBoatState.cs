using UnityEngine;

public class CollectingLootBoatState : BoatState, IProgressable
{
    private float currentCollectingTime = 0f;
    private float collectLootTime = 2f;

    private SwimmingDriftingLoot driftingLoot;

    public CollectingLootBoatState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        if (!boat.TargetDriftingLoot) {
            boat.SetState(BoatStateEnum.FindingLoot);
            return;
        }

        driftingLoot = boat.TargetDriftingLoot;
        if (!driftingLoot) {
            boat.SetState(BoatStateEnum.FindingLoot);
            return;
        }

        driftingLoot.StopMoving();
    }

    public override void Exit()
    {
        var container = boat.TargetDriftingLoot;
        if (!container) return;

        container.StartMoving();
    }

    public override void Tick()
    { 
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
        if (boat.Inventory.RemainingWeightInt > boat.FindLootMaxWeightThreshold) {
            boat.SetState(BoatStateEnum.FindingLoot);
        }
        else {
            boat.SetState(BoatStateEnum.MovingToDock);
        }
    }

    private bool TryCollectLoot()
    {
        if (!ShouldCollectLoot()) return false;

        var collectedLoot = driftingLoot.TakeItems();

        foreach (var loot in collectedLoot) {
            if (boat.Inventory.RemainingWeight <= 0f) break;

            boat.Inventory.AddItem(loot.Definition.ItemId, loot.Amount);
        }

        return true;
    }

    private bool ShouldCollectLoot()
    {
        if (!driftingLoot) return false;

        return true;
    }
}