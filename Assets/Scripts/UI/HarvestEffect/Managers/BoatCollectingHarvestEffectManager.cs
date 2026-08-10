using System.Collections.Generic;
using UnityEngine;

public class BoatCollectingHarvestEffectManager : HarvestItemEffectManager
{
    protected override void Subscribe()
    {
        base.Subscribe();

        CollectingLootBoatState.OnLootCollected += HandleBoatCollectedLoot;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        CollectingLootBoatState.OnLootCollected -= HandleBoatCollectedLoot;
    }

    private void HandleBoatCollectedLoot(Boat boat, List<ItemInstance> items)
    {
        if (boat == null) return;
        if (items == null) return;

        var positionHandler = boat.CollectingEffectPositionHandler;
        StartCoroutine(CreateWidgetsCoroutine(HarvestResourceWidgetPrefab, positionHandler.transform, positionHandler.StartTransform.localPosition, positionHandler.TargetTransform.localPosition, items, true));
    }
}