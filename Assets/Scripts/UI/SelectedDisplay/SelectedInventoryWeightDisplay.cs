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

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        SetInventory(null);
    }

    protected override void OnShow(SelectComponent selectComponent)
    {
        base.OnShow(selectComponent);

        textLocalizer.SetPlaceHolderLocalization(inventory);
    }

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) {
            SetInventory(null);
            return false;
        }

        var newInventory = selectComponent.GetComponent<Inventory>();
        SetInventory(newInventory);
        return true;
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