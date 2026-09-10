using UnityEngine;

public class StarvationInGameNotificationController : InGameNotificationController
{
    [SerializeField] private StarvationManager starvationManager;

    protected override void Subscribe()
    {
        base.Subscribe();

        starvationManager.OnStarvationStarted += HandleStarvationStarted;
        starvationManager.OnStarvationEnded += HandleStarvationEnded;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        starvationManager.OnStarvationStarted -= HandleStarvationStarted;
        starvationManager.OnStarvationEnded -= HandleStarvationEnded;
    }

    protected override bool ShouldNotificate()
    {
        return starvationManager.IsUnderStarvation;
    }

    private void HandleStarvationStarted()
    {
        UpdateNotification();
    }

    private void HandleStarvationEnded()
    {
        UpdateNotification();
    }
}