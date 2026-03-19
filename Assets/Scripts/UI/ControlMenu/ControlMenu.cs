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
        isOpened = true;
        OnOpen();
    }

    public void Close()
    {
        content.gameObject.SetActive(false);
        isOpened = false;
        OnClose();
        EventBus.InvokeWorkersMenuClosed();
    }

    protected abstract void OnOpen();
    protected abstract void OnClose();
}