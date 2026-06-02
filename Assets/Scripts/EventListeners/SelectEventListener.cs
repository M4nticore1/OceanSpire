using UnityEngine;

public class SelectEventListener : EventListener
{
    [SerializeField] private SelectComponent selectComponent;

    protected override void Subscribe()
    {
        base.Subscribe();

        selectComponent.OnSelected += OnSelected;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        selectComponent.OnSelected += OnSelected;
    }

    private void OnSelected()
    {
        HandleTriggered();
    }
}