using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SelectedDisplay : MonoBehaviour
{
    [SerializeField] private GameObject content;

    private SelectManager selectManager => SelectManager.Instance;
    private bool isSubscribed = false;

    public event Action<SelectedDisplay> OnShowed;
    public event Action<SelectedDisplay> OnHidden;

    private void OnEnable()
    {
        Subscribe();

        if (selectManager) {
            TryDisplay(selectManager.SelectedComponent);
            TryHide(selectManager.SelectedComponent);
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
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

        selectManager.OnComponentSelected += OnComponentSelected;
        selectManager.OnComponentDeselected += OnComponentDeselected;

        isSubscribed = true;
    }

    protected virtual void Unsubscribe()
    {
        if (!ShouldUnsubscribe()) return;

        selectManager.OnComponentSelected -= OnComponentSelected;
        selectManager.OnComponentDeselected -= OnComponentDeselected;

        isSubscribed = false;
    }

    protected virtual bool ShouldSubscribe()
    {
        if (isSubscribed) return false;
        if (!selectManager) return false;

        return true;
    }

    protected virtual bool ShouldUnsubscribe()
    {
        if (!isSubscribed) return false;
        if (!selectManager) return false;

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
        TryHide(selectComponent);
        TryDisplay(selectComponent);
    }

    private void OnComponentDeselected(SelectComponent selectComponentd)
    {
        TryHide(selectComponentd);
    }
}