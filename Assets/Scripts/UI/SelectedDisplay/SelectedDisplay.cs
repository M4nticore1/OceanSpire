using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SelectedDisplay : UIBehaviour
{
    private bool isSubscribed = false;

    protected override void Awake()
    {
        base.Awake();

        Subscribe();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        Subscribe();
        TryDisplay();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Unsubscribe();
        TryHide();
    }

    protected abstract void TryDisplay();
    protected abstract void TryHide();

    private void Subscribe()
    {
        if (isSubscribed) return;

        SelectManager.onComponentSelected += OnComponentSelected;
        SelectManager.onComponentDeselected += OnComponentDeselected;

        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed) return;

        SelectManager.onComponentSelected -= OnComponentSelected;
        SelectManager.onComponentDeselected -= OnComponentDeselected;

        isSubscribed = false;
    }

    private void OnComponentSelected(SelectComponent selected)
    {
        TryHide();
        TryDisplay();
    }

    private void OnComponentDeselected(SelectComponent selected)
    {
        TryHide();
    }
}