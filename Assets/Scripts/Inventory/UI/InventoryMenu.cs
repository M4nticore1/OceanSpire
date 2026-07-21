using System;
using UnityEngine;

public class InventoryMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private CustomButton closeButton;

    public event Action OnShowed;
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
        OnShowed?.Invoke();
    }

    public void Show(Inventory inventory)
    {
        slidePanel.Show();
        inventoryPanel.SetInventoryAndApply(inventory);
        InputStateManager.Instance.AddBlockTarget();

        Show();
    }

    public void Hide()
    {
        slidePanel.Hide();
        OnHidden?.Invoke();
    }

    private void OnHide()
    {
        InputStateManager.Instance.RemoveBlockTarget();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}