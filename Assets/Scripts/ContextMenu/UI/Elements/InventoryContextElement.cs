using UnityEngine;

public class InventoryContextElement : ContextElement
{
    [Header("Inventory")]
    [SerializeField] private InventoryMenu inventoryMenu;

    protected override void OnButtonClicked()
    {
        var contextMenuManager = ContextMenuManager.Instance;
        if (!contextMenuManager) return;

        var contextTarget = contextMenuManager.ContextMenuTarget;
        if (!contextTarget) return;

        var inventory = contextTarget.GetComponent<Inventory>();
        if (!inventory) return;

        inventoryMenu.Show(inventory);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        var inventory = target.GetComponent<Inventory>();
        if (!inventory) return false;

        return true;
    }
}