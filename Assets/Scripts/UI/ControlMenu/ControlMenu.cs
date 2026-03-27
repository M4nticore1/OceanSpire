using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ControlMenu : UIBehaviour, IOpenable
{
    protected bool isOpened = false;

    [SerializeField] private RectTransform content = null;
    [SerializeField] protected TextLocalizer selectedNameText = null;

    protected override void Start()
    {
        base.Start();

        Close();
    }

    // IOpenable
    public void Open()
    {
        content.gameObject.SetActive(true);
        UpdateMenu();
        UpdateName();

        InputStateManager.instance.SetGameplayInputBlocked(true);

        isOpened = true;
        OnOpen();
    }

    public void Close()
    {
        content.gameObject.SetActive(false);

        EventBus.InvokeWorkersMenuClosed();
        InputStateManager.instance.SetGameplayInputBlocked(false);

        isOpened = false;
        OnClose();
    }

    protected abstract void OnOpen();
    protected abstract void OnClose();
    protected abstract void UpdateMenu();
    protected abstract void UpdateName();
}