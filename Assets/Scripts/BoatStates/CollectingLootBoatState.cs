using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectingLootBoatState : BoatState, IProgressable
{
    private float currentCollectingTime = 0f;
    private float collectLootTime = 2f;

    public static event Action<Boat, List<ItemInstance>> OnLootCollected;

    public CollectingLootBoatState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        var driftingLoot = boat.TargetDriftingLoot;
        if (driftingLoot == null) {
            UpdateState();
            return;
        }

        driftingLoot.StopMoving();
    }

    public override void Exit()
    {
        var driftingLoot = boat.TargetDriftingLoot;
        if (driftingLoot == null) return;

        driftingLoot.StartMoving();
    }

    public override void Tick()
    {
        var driftingLoot = boat.TargetDriftingLoot;
        if (driftingLoot == null) {
            boat.RunUpdateStateCoroutine();
            return;
        }

        currentCollectingTime += Time.deltaTime;
        if (currentCollectingTime >= collectLootTime) {
            TryCollectLoot();
            UpdateState();
        }
    }

    public override void OnReachedPath()
    {

    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {
        
    }

    public float GetProgress()
    {
        return collectLootTime > 0 ? currentCollectingTime / collectLootTime : 1f;
    }

    private void UpdateState()
    {
        if (boat.IsOverweight()) {
            boat.SetState(BoatStateEnum.MovingToDock);
        }
        else {
            boat.SetState(BoatStateEnum.FindingLoot);
        }
    }

    private bool TryCollectLoot()
    {
        var driftingLoot = boat.TargetDriftingLoot;
        if (driftingLoot == null) {
            Debug.LogError($"[{nameof(CollectingLootBoatState)}] Target Drifting Loot is not valid!");
            return false;
        }

        var collectedLoot = driftingLoot.TakeItems();
        boat.RemoveTargetLoot();

        foreach (var loot in collectedLoot) {
            if (boat.Inventory.GetRemainingWeight() <= 0f) break;

            boat.Inventory.AddItemAmount(loot.Definition.ItemId, loot.Amount);
        }

        OnLootCollected?.Invoke(boat, collectedLoot);
        return true;
    }
}