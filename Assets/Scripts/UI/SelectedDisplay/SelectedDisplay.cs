using UnityEngine;

public abstract class SelectedDisplay : MonoBehaviour
{
    private bool isSubscribed = false;

    private void Awake()
    {
        Subscribe();
    }

    private void OnEnable()
    {
        Subscribe();
        TryDisplay();
    }

    private void OnDisable()
    {
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