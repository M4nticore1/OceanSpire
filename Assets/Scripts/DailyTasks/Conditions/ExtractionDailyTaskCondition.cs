using UnityEngine;

public class ExtractionDailyTaskCondition : DailyTaskCondition
{
    [Header("Extraction")]
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private CityStorageLoader cityStorageLoader;
    [SerializeField] private ItemDefinition conditionItem;

    protected override bool Subscribe()
    {
        cityStorage.Inventory.OnItemAmountAdded += OnAddedItemAmount;

        return true;
    }

    protected override bool Unsubscribe()
    {
        cityStorage.Inventory.OnItemAmountAdded -= OnAddedItemAmount;

        return true;
    }

    private void OnAddedItemAmount(ItemInstance item)
    {
        if (!cityStorageLoader.IsLoaded) return;
        if (item.Definition != conditionItem) return;

        InvokeProgressChanged(item.Amount);
    }
}