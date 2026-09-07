using UnityEngine;

public class StorageManagementList : ManagementList
{
    [SerializeField] private ItemWidget itemWidgetPrefab;
    [SerializeField] private ItemWidget stackWidget;
    [SerializeField] private ItemCategory itemCategory;
    [SerializeField] private ItemStackEnum itemStack;

    private void OnEnable()
    {
        CityStorage.Instance.Inventory.OnItemAmountChanged += OnItemAmountChanged;

        TryUpdateStack();
    }

    private void OnDisable()
    {
        CityStorage.Instance.Inventory.OnItemAmountChanged -= OnItemAmountChanged;
    }

    protected override void CreateWidgets()
    {
        foreach (var item in CityStorage.Instance.Inventory.Items) {
            if (item == null) continue;
            if (!item.Definition.ShowInStorage) continue;
            if (item.Definition.ItemCategory != itemCategory) continue;

            var widget = Instantiate(itemWidgetPrefab, LayoutGroup.transform);
            widget.SetItemDefinition(item.Definition);
            widget.AddAmount(item);
            widget.SetLimit(item.Stack);
            widget.SetIsCityItem(true);
        }
    }

    private void TryUpdateStack()
    {
        if (stackWidget == null) return;

        var stack = CityStorage.Instance.Inventory.GetStack(itemStack);
        if (stack == null) return;

        foreach (var item in stack.ItemAmounts) {
            if (item == null) continue;

            stackWidget.AddAmount(item);
        }

        stackWidget.SetLimit(stack);
    }

    private void OnItemAmountChanged(ItemInstance item)
    {
        if (item.Definition.ItemCategory != itemCategory) return;

        TryUpdateStack();
    }
}