using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ControlMenu : MonoBehaviour
{
    [SerializeField] private GameObject content;

    protected bool isOpened = false;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void Open()
    {
        content.SetActive(true);
        UpdateMenu();

        InputStateManager.Instance.SetGameplayInputBlocked(true);

        isOpened = true;
        OnOpen();
    }

    public void Close()
    {
        content.SetActive(false);

        InputStateManager.Instance.SetGameplayInputBlocked(false);

        isOpened = false;
        OnClose();
    }

    protected abstract void OnOpen();
    protected abstract void OnClose();
    protected abstract void UpdateMenu();

    protected virtual void Subscribe()
    {

    }

    protected virtual void Unsubscribe()
    {

    }

    protected virtual bool ShouldSubscribe()
    {
        return true;
    }

    protected virtual bool ShouldUnsubscribe()
    {
        return true;
    }
}