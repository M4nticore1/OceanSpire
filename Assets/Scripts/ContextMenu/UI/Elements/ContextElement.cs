using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ContextElement : UIBehaviour
{
    [Header("Main")]
    [SerializeField] private GameObject content;
    [SerializeField] protected CustomButton button;

    public bool IsOpened { get; protected set; } = false;
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
    }

    protected virtual void Subscribe()
    {
        button.OnReleased.AddListener(OnButtonClicked);
        ContextMenuManager.Instance.OnContextMenuTargetSelected += OnTargetSelected;
    }

    protected virtual void Unsubscribe()
    {
        button.OnReleased.RemoveListener(OnButtonClicked);
        ContextMenuManager.Instance.OnContextMenuTargetSelected -= OnTargetSelected;
    }

    protected virtual void Show()
    {
        gameObject.SetActive(true);
        UpdateButtonEnabled();
        IsOpened = true;
    }

    protected virtual void Hide()
    {
        gameObject.SetActive(false);
        IsOpened = false;
    }

    protected virtual bool ShouldEnableButton()
    {
        return true;
    }

    protected abstract void OnButtonClicked();

    protected abstract bool ShouldShow(ContextMenuTarget target);

    protected void UpdateActive(ContextMenuTarget target)
    {
        if (!target) return;

        if (ShouldShow(target)) {
            Show();
        }
        else {
            Hide();
        }
    }

    protected void UpdateButtonEnabled()
    {
        button.SetState(ShouldEnableButton() ? CustomButtonState.Idle : CustomButtonState.Disabled);
    }

    private void OnTargetSelected(ContextMenuTarget target)
    {
        UpdateActive(target);
    }

    private bool TrySubscribe()
    {
        if (isSubscribed) return false;
        if (!ContextMenuManager.Instance) return false;

        Subscribe();
        isSubscribed = true;

        return true;
    }

    private bool TryUnsubscribe()
    {
        if (!isSubscribed) return false;
        if (!ContextMenuManager.Instance) return false;

        Subscribe();
        isSubscribed = false;

        return true;
    }
}