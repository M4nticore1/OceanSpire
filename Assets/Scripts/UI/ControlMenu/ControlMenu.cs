using System;
using UnityEngine;

public abstract class ControlMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private GameObject content;
    [SerializeField] private CustomButton closeButton;

    public bool IsShowed { get; private set; } = false;

    public event Action OnShowed;
    public event Action OnHidden;

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    protected virtual void Subscribe()
    {
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    protected virtual void Unsubscribe()
    {
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
    }

    protected virtual bool ShouldSubscribe()
    {
        return true;
    }

    protected virtual bool ShouldUnsubscribe()
    {
        return true;
    }

    public void Show()
    {
        IsShowed = true;
        content.SetActive(true);
        UpdateMenu();

        InputStateManager.Instance.AddBlockTarget(this);
        OnShow();

        OnShowed?.Invoke();
    }

    public void Hide()
    {
        IsShowed = false;
        content.SetActive(false);

        InputStateManager.Instance.RemoveBlockTarget(this);
        OnHide();

        OnHidden?.Invoke();
    }

    protected abstract void OnShow();

    protected abstract void OnHide();

    protected abstract void UpdateMenu();

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}