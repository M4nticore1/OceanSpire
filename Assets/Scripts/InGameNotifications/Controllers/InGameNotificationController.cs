using System;
using UnityEngine;

public abstract class InGameNotificationController : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private InGameNotificationsPanel notificationsPanel;
    public InGameNotificationsPanel NotificationsPanel => notificationsPanel;

    [SerializeField] private InGameNotificationSeverityDefinition severityDefinition;
    public InGameNotificationSeverityDefinition SeverityDefinition => severityDefinition;

    [SerializeField] private int priority = 50;
    public int Priority => priority;

    [Header("Data")]
    [SerializeField] private LocalizationItem nameLocalizationItem;
    public LocalizationItem NameLocalizationItem => nameLocalizationItem;

    [SerializeField] private LocalizationItem descriptionLocalizationItem;
    public LocalizationItem DescriptionLocalizationItem => descriptionLocalizationItem;

    protected virtual void Awake()
    {
        if (severityDefinition == null) {
            Debug.LogError($"[{nameof(InGameNotificationData)}] SeverityDefinition is not valid at {this}!");
        }
        if (nameLocalizationItem == null) {
            Debug.LogError($"[{nameof(InGameNotificationData)}] NameLocalization is not valid at {this}!");
        }
        if (descriptionLocalizationItem == null) {
            Debug.LogError($"[{nameof(InGameNotificationData)}] DescriptionLocaliztion is not valid at {this}!");
        }
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
        return new InGameNotificationData(nameLocalizationItem, descriptionLocalizationItem, severityDefinition, priority);
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