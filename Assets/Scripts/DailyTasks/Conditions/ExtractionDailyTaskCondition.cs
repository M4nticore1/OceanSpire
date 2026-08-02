using UnityEngine;

public class ExtractionDailyTaskCondition : DailyTaskCondition
{
    [Header("Extraction")]
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private CityStorageLoader cityStorageLoader;
    [SerializeField] private ItemDefinition conditionItem;

    protected override bool Subscribe()
    {
        UnloadingLootBoatState.OnLootUnloaded += OnAddedItemAmount;

        return true;
    }

    protected override bool Unsubscribe()
    {
        UnloadingLootBoatState.OnLootUnloaded += OnAddedItemAmount;

        return true;
    }

    private void OnAddedItemAmount(Boat boat, ItemInstance item)
    {
        if (item == null) return;

        var definition = item.Definition;
        if (!definition) return;

        if (definition.ItemId != conditionItem.ItemId) return;

        InvokeProgressChanged(item.Amount);
    }
}