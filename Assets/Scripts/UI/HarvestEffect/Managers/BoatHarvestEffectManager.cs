using UnityEngine;

public class BoatHarvestEffectManager : HarvestItemEffectManager
{
    protected override void Subscribe()
    {
        base.Subscribe();

        UnloadingLootBoatState.OnLootUnloaded += OnBoatUnloadedResource;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        UnloadingLootBoatState.OnLootUnloaded -= OnBoatUnloadedResource;
    }

    private void OnBoatUnloadedResource(Boat boat, ItemInstance item)
    {
        if (!boat) return;
        if (item == null) return;

        var positionHandler = boat.HarvestEffectPositionHandler;
        TryCreateWidget(item, boat.Canvas.transform, positionHandler.StartTransform.position, positionHandler.TargetTransform.position);
    }
}