using System;
using UnityEngine;

public class InventoryMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private CustomButton closeButton;

    public event Action OnShown;
    public event Action OnHidden;

    private void OnEnable()
    {
        slidePanel.OnHidden += OnHide;
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    private void OnDisable()
    {
        slidePanel.OnHidden -= OnHide;
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
    }

    public void Show()
    {
        OnShown?.Invoke();
    }

    public void Show(Inventory inventory)
    {
        slidePanel.Show();
        inventoryPanel.SetInventoryAndApply(inventory);
        InputStateManager.Instance.SetGameplayInputBlocked(true);

        Show();
    }

    public void Hide()
    {
        slidePanel.Hide();
        OnHidden?.Invoke();
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