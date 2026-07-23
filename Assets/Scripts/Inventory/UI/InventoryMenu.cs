using System;
using UnityEngine;

public class InventoryMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private CustomButton closeButton;

    public bool IsShowed { get; private set; } = false;

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
        Debug.Log("Show");
        IsShowed = true;
        slidePanel.Show();
        InputStateManager.Instance.AddBlockTarget(this);

        OnShowed?.Invoke();
    }

    public void Show(Inventory inventory)
    {
        inventoryPanel.SetInventoryAndApply(inventory);
        Show();
    }

    public void Hide()
    {
        Debug.Log("Hide");
        slidePanel.Hide();
    }

    private void OnHide()
    {
        IsShowed = false;
        InputStateManager.Instance.RemoveBlockTarget(this);
        OnHidden?.Invoke();
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}