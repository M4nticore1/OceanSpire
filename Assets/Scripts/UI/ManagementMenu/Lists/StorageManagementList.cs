using UnityEngine;

public class StorageManagementList : ManagementList
{
    [SerializeField] private ResourceWidget itemWidgetPrefab;
    [SerializeField] private ResourceWidget stackWidget;
    [SerializeField] private ItemCategory itemCategory;
    [SerializeField] private ItemStackEnum itemStack;

    private void OnEnable()
    {
        CityStorage.Instance.Inventory.onItemAmountChanged += OnItemAmountChanged;

        TryUpdateStack();
    }

    private void OnDisable()
    {
        CityStorage.Instance.Inventory.onItemAmountChanged -= OnItemAmountChanged;
    }

    protected override void CreateWidgets()
    {
        foreach (var item in CityStorage.Instance.Inventory.Items) {
            if (!item.Definition.ShowInStorage) continue;
            if (item.Definition.ItemCategory != itemCategory) continue;

            var widget = Instantiate(itemWidgetPrefab, LayoutGroup.transform);
            widget.SetItemAndApply(item.Definition);
            widget.SetAmount(item);
            widget.SetLimit(item.Stack);
        }
    }

    private void TryUpdateStack()
    {
        if (!stackWidget) return;

        int amount = CityStorage.Instance.Inventory.GetStack(itemStack).TotalAmount;
        int limit = CityStorage.Instance.Inventory.GetStack(itemStack).Amount;

        stackWidget.SetAmountText(amount, limit);
    }

    private void OnItemAmountChanged(ItemInstance item)
    {
        if (item.Definition.ItemCategory != itemCategory) return;

        TryUpdateStack();
    }
}