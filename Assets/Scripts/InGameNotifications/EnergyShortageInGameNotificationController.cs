using UnityEngine;

public class EnergyShortageInGameNotificationController : InGameNotificationController
{
    [SerializeField] private EnergyShortageManager energyShortageManager;

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

    protected override bool ShouldNotificate()
    {
        return energyShortageManager.IsUnderEnergyShortage;
    }

    private void HandleEnergyShortageStarted()
    {
        UpdateNotification();
    }

    private void HandleEnergyShortageEnded()
    {
        UpdateNotification();
    }
}