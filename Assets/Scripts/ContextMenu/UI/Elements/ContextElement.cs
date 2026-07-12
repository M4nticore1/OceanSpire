using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ContextElement : UIBehaviour
{
    [Header("Main")]
    [SerializeField] private GameObject content;
    [SerializeField] protected CustomButton button;

    private bool isSubscribed = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        TrySubscribe();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        TryUnsubscribe();
    }

    protected override void Start()
    {
        base.Start();

        TrySubscribe();
        ContextMenuManager.Instance.OnContextMenuTargetSelected += OnTargetSelected;
    }

    protected virtual bool Subscribe()
    {
        button.OnReleased.AddListener(OnButtonClicked);

        return true;
    }

    protected virtual bool Unsubscribe()
    {
        button.OnReleased.RemoveListener(OnButtonClicked);

        return true;
    }

    protected abstract void OnShowed();

    protected abstract void OnButtonClicked();

    protected abstract bool ShouldShow(ContextMenuTarget target);

    protected void Show()
    {
        gameObject.SetActive(true);
        OnShowed();
    }

    protected void Hide()
    {
        gameObject.SetActive(false);
    }

    protected void OnTargetSelected(ContextMenuTarget target)
    {
        if (!target) return;

        if (ShouldShow(target)) {
            Show();
        }
        else {
            Hide();
        }
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;
        if (!Subscribe()) return;

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (!Unsubscribe()) return;

        isSubscribed = false;
    }
}