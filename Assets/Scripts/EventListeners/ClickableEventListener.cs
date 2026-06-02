using UnityEngine;

public class ClickableEventListener : EventListener
{
    [SerializeField] private MonoBehaviour clickable;
    private IClickable Clickable => clickable.GetComponent<IClickable>();

    protected override void Subscribe()
    {
        base.Subscribe();

        if (Clickable == null) {
            Debug.Log($"Clickable not found at {name}");
            return;
        }

        Clickable.OnClicked += OnClicked;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        if (Clickable == null) return;

        Clickable.OnClicked -= OnClicked;
    }

    private void OnClicked()
    {
        HandleTriggered();
    }
}