using UnityEngine;

public class InventoryMenu : MonoBehaviour
{
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private CustomButton closeButton;

    private void OnEnable()
    {
        slidePanel.OnClosed += OnHide;
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    private void OnDisable()
    {
        slidePanel.OnClosed -= OnHide;
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
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

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}