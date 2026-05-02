using UnityEngine;

public class ExtractionDailyTaskCondition : DailyTaskCondition
{
    [SerializeField] private ItemData conditionItem;

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
        if (!ItemsLoader.Instance) return;
        if (!ItemsLoader.Instance.IsLoaded) return;
        if (item.ItemData != conditionItem) return;

        InvokeProgressChanged(item.Amount);
    }
}