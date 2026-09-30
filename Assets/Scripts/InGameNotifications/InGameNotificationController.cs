using System;
using UnityEngine;

public abstract class InGameNotificationController : MonoBehaviour
{
    [SerializeField] private InGameNotificationWidget notificationPrefab;

    [SerializeField] private InGameNotificationsPanel notificationsPanel;
    public InGameNotificationsPanel NotificationsPanel => notificationsPanel;

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        UpdateNotification();
    }

    protected virtual void Subscribe()
    {

    }

    protected virtual void Unsubscribe()
    {

    }

    protected abstract bool ShouldNotificate();

    protected void UpdateNotification()
    {
        if (ShouldNotificate()) {
            notificationsPanel.ShowNotification(notificationPrefab);
        }
        else {
            notificationsPanel.HideNotification(notificationPrefab);
        }
    }
}