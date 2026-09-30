using UnityEngine;

public class EnergyShortageInGameNotificationController : InGameNotificationController
{
    [SerializeField] private EnergyShortageManager energyShortageManager;

    private InGameNotificationData notificationData;

    protected override void Awake()
    {
        base.Awake();

        notificationData = GetNotificationData();
    }

    protected override void Subscribe()
    {
        base.Subscribe();

        energyShortageManager.OnEnergyShortageStarted += HandleEnergyShortageStarted;
        energyShortageManager.OnEnergyShortageEnded += HandleEnergyShortageEnded;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        energyShortageManager.OnEnergyShortageStarted -= HandleEnergyShortageStarted;
        energyShortageManager.OnEnergyShortageEnded -= HandleEnergyShortageEnded;
    }

    private void HandleEnergyShortageStarted()
    {
        ShowNotification(notificationData);
    }

    private void HandleEnergyShortageEnded()
    {
        HideNotification(notificationData);
    }
}