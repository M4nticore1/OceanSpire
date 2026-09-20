using UnityEngine;

public class StarvationInGameNotificationController : InGameNotificationController
{
    [SerializeField] private StarvationManager starvationManager;

    private InGameNotificationData notificationData;

    protected override void Awake()
    {
        base.Awake();

        notificationData = GetNotificationData();
    }

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

    private void HandleStarvationStarted()
    {
        ShowNotification(notificationData);
    }

    private void HandleStarvationEnded()
    {
        HideNotification(notificationData);
    }
}