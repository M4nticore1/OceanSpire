using System;
using UnityEngine;

public abstract class InGameNotificationController : MonoBehaviour
{
    [SerializeField] private InGameNotificationsPanel notificationsPanel;
    public InGameNotificationsPanel NotificationsPanel => notificationsPanel;

    [Header("Data")]
    [SerializeField] private LocalizationItem nameLocalizationItem;
    [SerializeField] private LocalizationItem descriptionLocalizationItem;

    protected virtual void Awake()
    {

    }

    protected virtual void OnEnable()
    {
        Subscribe();
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();
    }

    protected virtual void Subscribe()
    {

    }

    protected virtual void Unsubscribe()
    {

    }

    protected virtual InGameNotificationData GetNotificationData()
    {
        return new InGameNotificationData(nameLocalizationItem, descriptionLocalizationItem);
    }

    protected void ShowNotification(InGameNotificationData notificationData)
    {
        if (notificationsPanel == null) return;

        notificationsPanel.ShowNotification(notificationData);
    }

    protected void HideNotification(InGameNotificationData notificationData)
    {
        if (notificationsPanel == null) return;

        notificationsPanel.HideNotification(notificationData);
    }
}