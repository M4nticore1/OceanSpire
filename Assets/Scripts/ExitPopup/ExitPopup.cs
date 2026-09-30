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
        exitButton.OnReleased.AddListener(HandleExitButtonClicked);
        cancelButton.OnReleased.AddListener(HandleCancelButtonClicked);
    }

    private void OnDisable()
    {
        exitButton.OnReleased.RemoveListener(HandleExitButtonClicked);
        cancelButton.OnReleased.RemoveListener(HandleCancelButtonClicked);
    }

    public void Show()
    {
        slidePanel.Show();
    }

    public void Hide()
    {
        slidePanel.Hide();
    }

    protected abstract void HandleExitButtonClicked();

    private void HandleCancelButtonClicked()
    {
        Hide();
    }
}