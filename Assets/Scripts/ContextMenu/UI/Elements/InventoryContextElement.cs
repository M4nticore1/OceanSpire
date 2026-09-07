using UnityEngine;

public class InventoryContextElement : ContextElement
{
    [Header("Inventory")]
    [SerializeField] private InventoryMenu inventoryMenu;
    private Inventory inventory;

    protected override void OnButtonClicked()
    {
        inventoryMenu.Show(inventory);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        inventory = target.GetComponent<Inventory>();
        if (inventory == null) return false;

        if (inventory.IgnoreContextMenu) return false;

        return true;
    }
}