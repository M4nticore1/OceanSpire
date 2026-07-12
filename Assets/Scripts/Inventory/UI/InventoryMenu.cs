using UnityEngine;

public class InventoryMenu : MonoBehaviour
{
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private SlidePanel slidePanel;

    private void OnEnable()
    {
        slidePanel.OnClosed += OnHide;
    }

    private void OnDisable()
    {
        slidePanel.OnClosed -= OnHide;
    }

    public void Show(Inventory inventory)
    {
        slidePanel.Open();
        inventoryPanel.SetInventoryAndApply(inventory);

        InputStateManager.Instance.SetGameplayInputBlocked(true);
    }

    public void Hide()
    {
        slidePanel.Close();
    }

    private void OnHide()
    {
        InputStateManager.Instance.SetGameplayInputBlocked(false);
    }
}