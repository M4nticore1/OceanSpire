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
        boat.targetLootContainer.StopMoving();
    }

    public override void Exit()
    {
        LootContainer container = boat.targetLootContainer;
        if (!container) return;

        container.StartMoving();
    }

    public override void Process()
    {
        boat.ProcessDrainHealth();

        currentCollectingTime += Time.deltaTime;

        if (currentCollectingTime <= collectLootTime) return;

        CollectLoot();

        if (boat.Inventory.RemainingWeight > 0) {
            boat.SetState(BoatStateEnum.FindingLoot);
        }
        else {
            boat.SetState(BoatStateEnum.ReturningToDock);
        }
    }

    public override void HandleReachedPath()
    {

    }

    private void CollectLoot()
    {
        float remainingWeight = boat.Inventory.MaxWeight - boat.Inventory.CurrentWeight;
        List<ItemInstance> collectedLoot = boat.targetLootContainer.TakeItems(remainingWeight);

        foreach (var loot in collectedLoot) {
            if (boat.Inventory.RemainingWeight <= 0) break;

            ItemData data = loot.ItemData;
            int id = loot.ItemData.ItemId;
            int amountToTake = math.min(loot.Amount, (int)(boat.Inventory.RemainingWeight / loot.ItemData.Weight));

            boat.Inventory.AddItem(id, amountToTake, amountToTake);
        }
    }
}