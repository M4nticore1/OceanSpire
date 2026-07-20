using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SelectedDisplay : UIBehaviour
{
    [SerializeField] private GameObject content;

    private SelectManager selectManager => SelectManager.Instance;
    private bool isSubscribed = false;

    public event Action<SelectedDisplay> OnShowed;
    public event Action<SelectedDisplay> OnHidden;

    protected override void OnEnable()
    {
        base.OnEnable();

        Subscribe();

        if (selectManager) {
            TryDisplay(selectManager.SelectedComponent);
            TryHide(selectManager.SelectedComponent);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Unsubscribe();
    }

    protected override void Start()
    {
        base.Start();

        Subscribe();
    }

    public void Display(SelectComponent selectComponent)
    {
        OnShow(selectComponent);
        OnShowed?.Invoke(this);
    }

    public void Hide(SelectComponent selectComponent)
    {
         OnHide(selectComponent);
         OnHidden?.Invoke(this);
    }

    protected virtual void OnShow(SelectComponent selectComponent)
    {
        if (content) {
            content.SetActive(true);
        }
    }

    protected virtual void OnHide(SelectComponent selectComponent)
    {
        if (content) {
            content.SetActive(false);
        }
    }

    protected virtual void Subscribe()
    {
        if (!ShouldSubscribe()) return;

        SelectManager.Instance.OnComponentSelected += OnComponentSelected;
        SelectManager.Instance.OnComponentDeselected += OnComponentDeselected;

        isSubscribed = true;
    }

    protected virtual void Unsubscribe()
    {
        if (!ShouldUnsubscribe()) return;

        SelectManager.Instance.OnComponentSelected -= OnComponentSelected;
        SelectManager.Instance.OnComponentDeselected -= OnComponentDeselected;

        isSubscribed = false;
    }

    protected virtual bool ShouldSubscribe()
    {
        if (isSubscribed) return false;
        if (!SelectManager.Instance) return false;

        return true;
    }

    protected virtual bool ShouldUnsubscribe()
    {
        if (!isSubscribed) return false;
        if (!SelectManager.Instance) return false;

        return true;
    }

    protected abstract bool ShouldDisplay(SelectComponent selectComponent);

    private void TryDisplay(SelectComponent selectComponent)
    {
        if (!ShouldDisplay(selectComponent)) return;

        Display(selectComponent);
    }

    private void TryHide(SelectComponent selectComponent)
    {
        if (ShouldDisplay(selectComponent)) return;

        Hide(selectComponent);
    }

    private void OnComponentSelected(SelectComponent selectComponent)
    {
        if (!selectComponent) return;

        TryHide(selectComponent);
        TryDisplay(selectComponent);
    }

    private void OnComponentDeselected(SelectComponent selectComponentd)
    {
        if (!selectComponentd) return;

        TryHide(selectComponentd);
    }
}