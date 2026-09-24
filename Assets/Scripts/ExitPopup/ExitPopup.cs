using System;
using UnityEngine;

public abstract class ExitPopup : MonoBehaviour, IOpenable
{
    [SerializeField] private SlideAnimatedPanel slidePanel;
    [SerializeField] private CustomButton exitButton;
    [SerializeField] private CustomButton cancelButton;

    public bool IsShown => slidePanel.IsShown;

    public event Action OnShown;
    public event Action OnHidden;

    private void OnEnable()
    {
        slidePanel.OnHidden += HandleSlidePanelHidden;
        exitButton.OnReleased.AddListener(HandleExitButtonClicked);
        cancelButton.OnReleased.AddListener(HandleCancelButtonClicked);
    }

    private void OnDisable()
    {
        slidePanel.OnHidden -= HandleSlidePanelHidden;
        exitButton.OnReleased.RemoveListener(HandleExitButtonClicked);
        cancelButton.OnReleased.RemoveListener(HandleCancelButtonClicked);
    }

    public void Show()
    {
        slidePanel.Show();

        InputStateManager.Instance.AddInputBlockTarget(this);
    }

    public void Hide()
    {
        slidePanel.Hide();
    }

    protected abstract void HandleExitButtonClicked();

    private void HandleSlidePanelHidden()
    {
        InputStateManager.Instance.RemoveBlockTarget(this);
    }

    private void HandleCancelButtonClicked()
    {
        Hide();
    }
}