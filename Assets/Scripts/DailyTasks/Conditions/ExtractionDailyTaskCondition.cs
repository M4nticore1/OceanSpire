using UnityEngine;

public class ExtractionDailyTaskCondition : DailyTaskCondition
{
    [SerializeField] private ItemDefinition conditionItem;

    protected override bool Subscribe()
    {
        if (!CityStorage.Instance) return false;

        CityStorage.Instance.Inventory.onAddedItemAmount += OnAddedItemAmount;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!CityStorage.Instance) return false;

        CityStorage.Instance.Inventory.onAddedItemAmount -= OnAddedItemAmount;

        return true;
    }

    private void OnAddedItemAmount(ItemInstance item)
    {
        if (!CityStorageLoader.Instance) return;
        if (!CityStorageLoader.Instance.IsLoaded) return;
        if (item.Definition != conditionItem) return;

        InvokeProgressChanged(item.Amount);
    }
}