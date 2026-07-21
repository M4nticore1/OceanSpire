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

    private void OnAddedItemAmount(ItemID id, int amount)
    {
        if (id != conditionItem.ItemId) return;

        InvokeProgressChanged(amount);
    }
}