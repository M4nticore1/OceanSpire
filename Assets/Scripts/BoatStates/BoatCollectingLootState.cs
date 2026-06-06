using Unity.Mathematics;
using UnityEngine;

public class BoatCollectingLootState : BoatState
{
    private float currentCollectingTime = 0f;
    private float collectLootTime = 2f;

    public BoatCollectingLootState(Boat boat) : base(boat)
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

        if (boat.Inventory.RemainingWeight > 0) {
            boat.SetState(BoatStateEnum.FindingLoot);
        }
        else {
            boat.SetState(BoatStateEnum.MovingToDock);
        }
    }

    public override void OnReachedPath()
    {

    }

    private bool TryStopDriftingLoot()
    {
        if (!boat) return false;
        if (!boat.TargetDriftingLoot) return false;
        if (!boat.TargetDriftingLoot.IsMoving) return false;

        boat.TargetDriftingLoot.StopMoving();
        return true;
    }

    private void TryCollectLoot()
    {
        if (!ShouldCollectLoot()) return;

        var collectedLoot = boat.TargetDriftingLoot.TakeItems();

        foreach (var loot in collectedLoot) {
            if (boat.Inventory.RemainingWeight <= 0) break;

            var data = loot.Definition;
            int id = loot.Definition.ItemId;
            int amountToTake = math.min(loot.Amount, (int)(boat.Inventory.RemainingWeight / loot.Definition.Weight));

            boat.Inventory.AddItem(id, amountToTake);
        }
    }

    private bool ShouldCollectLoot()
    {
        if (!boat) return false;
        if (!boat.TargetDriftingLoot) return false;

        return true;
    }
}