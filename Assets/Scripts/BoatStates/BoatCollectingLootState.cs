using System.Collections.Generic;
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
        boat.TargetDriftingLoot.StopMoving();
    }

    public override void Exit()
    {
        DriftingLoot container = boat.TargetDriftingLoot;
        if (!container) return;

        container.StartMoving();
    }

    public override void Tick()
    {
        currentCollectingTime += Time.deltaTime;

        if (currentCollectingTime <= collectLootTime) return;

        CollectLoot();

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

    private void CollectLoot()
    {
        var collectedLoot = boat.TargetDriftingLoot.TakeItems();

        foreach (var loot in collectedLoot) {
            if (boat.Inventory.RemainingWeight <= 0) break;

            var data = loot.Definition;
            int id = loot.Definition.ItemId;
            int amountToTake = math.min(loot.Amount, (int)(boat.Inventory.RemainingWeight / loot.Definition.Weight));

            boat.Inventory.AddItem(id, amountToTake);
        }
    }
}