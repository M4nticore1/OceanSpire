using UnityEngine;

public class SelectedInventoryWeightDisplay : SelectedDisplay
{
    [SerializeField] private TextLocalizer textLocalizer;

    private Inventory inventory;

    private void SubscribeInventory(Inventory inventory)
    {
        if (!inventory) return;

        inventory.OnItemAmountChanged += OnItemAmountChanged;
    }

    private void UnsubscribeInventory(Inventory inventory)
    {
        if (!inventory) return;

        inventory.OnItemAmountChanged -= OnItemAmountChanged;
    }

    protected override void OnUnsubscribe()
    {
        base.OnUnsubscribe();

        SetInventory(null);
    }

    protected override void OnShow(SelectComponent selectComponent)
    {
        base.OnShow(selectComponent);

        textLocalizer.SetPlaceHolderLocalization(inventory);
    }

    protected override void OnHide(SelectComponent selectComponent)
    {
        base.OnHide(selectComponent);

        SetInventory(null);
    }

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) {
            SetInventory(null);
            return false;
        }

        var inventory = selectComponent.GetComponent<Inventory>();
        SetInventory(inventory);

        return inventory != null;
    }

    private void SetInventory(Inventory inventory)
    {
        if (this.inventory == inventory) return;

        UnsubscribeInventory(this.inventory);
        this.inventory = inventory;
        SubscribeInventory(inventory);
    }

    private void OnItemAmountChanged(ItemInstance item)
    {
        textLocalizer.UpdateText();
    }
}