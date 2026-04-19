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
        Display();
    }

    private void OnDisable()
    {
        Unsubscribe();
        Hide();
    }

    protected abstract void Display();
    protected abstract void Hide();

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
        Hide();
        Display();
    }

    private void OnComponentDeselected(SelectComponent selected)
    {
        Hide();
    }
}