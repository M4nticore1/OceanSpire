using UnityEngine;

public class ButtonClickEventListener : EventListener
{
    [SerializeField] private CustomButton button;

    protected override void Subscribe()
    {
        base.Subscribe();

        button.OnReleased.AddListener(OnClicked);
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        button.OnReleased.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        HandleTriggered();
    }
}