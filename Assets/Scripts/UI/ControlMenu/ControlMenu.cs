using System;
using UnityEngine;

public abstract class ControlMenu : MonoBehaviour, IOpenable
{
    [SerializeField] private GameObject content;
    [SerializeField] private CustomButton closeButton;

    protected bool isOpened = false;

    public event Action OnShown;
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
        isOpened = true;
        content.SetActive(true);
        UpdateMenu();

        InputStateManager.Instance.AddBlockTarget();
        OnShow();

        OnShown?.Invoke();
    }

    public void Hide()
    {
        isOpened = false;
        content.SetActive(false);

        InputStateManager.Instance.RemoveBlockTarget();
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