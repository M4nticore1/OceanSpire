using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ContextElement : MonoBehaviour, IOpenable
{
    [Header("Main")]
    [SerializeField] private GameObject content;

    [SerializeField] private int siblingIndex = 0;
    public int SiblingIndex => siblingIndex;

    [SerializeField] protected CustomButton button;

    private ContextMenuManager contextMenuManager => ContextMenuManager.Instance;

    public bool IsShowed { get; protected set; } = false;
    private bool isSubscribed = false;

    public event Action OnShowed;
    public event Action OnHidden;

    public static event Action<ContextElement> OnElementShowed;
    public static event Action<ContextElement> OnElementHidden;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    protected virtual void Subscribe()
    {
        if (button)
            button.OnReleased.AddListener(OnButtonClicked);
        else
            Debug.LogError($"[{nameof(ContextElement)}] Button is not valid at {name}!");

        if (contextMenuManager)
            contextMenuManager.OnContextMenuTargetSelected += OnTargetSelected;
        else
            Debug.LogError($"[{nameof(ContextElement)}] Context Menu Manager is not valid at {name}!");
    }

    protected virtual void Unsubscribe()
    {
        if (button)
            button.OnReleased.RemoveListener(OnButtonClicked);
        else
            Debug.LogError($"[{nameof(ContextElement)}] Button is not valid at {name}!");

        if (contextMenuManager)
            contextMenuManager.OnContextMenuTargetSelected -= OnTargetSelected;
        else
            Debug.LogError($"[{nameof(ContextElement)}] Context Menu Manager is not valid at {name}!");
    }

    public void Show()
    {
        if (IsShowed) return;

        OnShow();

        OnShowed?.Invoke();
        OnElementShowed?.Invoke(this);
    }

    public void Hide()
    {
        OnHide();

        OnHidden?.Invoke();
        OnElementHidden?.Invoke(this);
    }

    protected virtual void OnShow()
    {
        IsShowed = true;
        content.SetActive(true);
        UpdateButtonEnabled();
    }

    protected virtual void OnHide()
    {
        IsShowed = false;
        content.SetActive(false);
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
        if (!target) return;

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

        Unsubscribe();
        isSubscribed = false;
        return true;
    }
}