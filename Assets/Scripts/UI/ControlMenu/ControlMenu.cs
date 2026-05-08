using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ControlMenu : UIBehaviour, IOpenable
{
    protected bool isOpened = false;

    protected override void Start()
    {
        base.Start();

        Close();
    }

    // IOpenable
    public void Open()
    {
        gameObject.SetActive(true);
        UpdateMenu();

        InputStateManager.Instance.SetGameplayInputBlocked(true);

        isOpened = true;
        OnOpen();
    }

    public void Close()
    {
        gameObject.SetActive(false);

        EventBus.InvokeWorkersMenuClosed();
        InputStateManager.Instance.SetGameplayInputBlocked(false);

        isOpened = false;
        OnClose();
    }

    protected abstract void OnOpen();
    protected abstract void OnClose();
    protected abstract void UpdateMenu();
}