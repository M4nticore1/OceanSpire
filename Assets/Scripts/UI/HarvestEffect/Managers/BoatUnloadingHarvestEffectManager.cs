using UnityEngine;

public class BoatUnloadingHarvestEffectManager : HarvestItemEffectManager
{
    protected override void Subscribe()
    {
        base.Subscribe();

        UnloadingLootBoatState.OnLootUnloaded += HandleBoatUnloadedResource;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        UnloadingLootBoatState.OnLootUnloaded -= HandleBoatUnloadedResource;
    }

    private void HandleBoatUnloadedResource(Boat boat, ItemInstance item)
    {
        if (boat == null) return;
        if (item == null) return;

        var positionHandler = boat.UnloadingEffectPositionHandler;
        TryCreateWidget(HarvestResourceWidgetPrefab, positionHandler.transform, positionHandler.StartTransform.localPosition, positionHandler.TargetTransform.localPosition, item, true);
    }
}