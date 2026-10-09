using UnityEngine;

public class ExtractionDailyTaskController : DailyTaskController
{
    [Header("Extraction")]
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private CityStorageLoader cityStorageLoader;
    [SerializeField] private ItemDefinition conditionItem;

    protected override bool Subscribe()
    {
        if (!base.Subscribe()) return false;

        UnloadingLootBoatState.OnLootUnloaded += OnAddedItemAmount;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!base.Unsubscribe()) return false;

        UnloadingLootBoatState.OnLootUnloaded += OnAddedItemAmount;

        return true;
    }

    private void OnAddedItemAmount(Boat boat, ItemInstance item)
    {
        if (item == null) return;

        var definition = item.Definition;
        if (definition == null) return;

        if (definition.ItemId != conditionItem.ItemId) return;

        AddTaskProgress(item.Amount);
    }
}