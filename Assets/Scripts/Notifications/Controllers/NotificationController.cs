using UnityEngine;

public abstract class NotificationController : MonoBehaviour
{
    [SerializeField] private NotificationsManager notificationsManager;
    public NotificationsManager NotificationsManager => notificationsManager;

    [SerializeField] private LocalizationItem labelLocalizationItem;
    public LocalizationItem LabelLocalizationItem => labelLocalizationItem;

    [SerializeField] private LocalizationItem bodyLocalizationItem;
    public LocalizationItem BodyLocalizationItem => bodyLocalizationItem;

    [SerializeField] private LocalizationItem subtitleLocalizationItem;
    public LocalizationItem SubtitleLocalizationItem => subtitleLocalizationItem;

    private void OnEnable()
    {
        notificationsManager.OnNotificationsCanceled += OnNotificationsCanceled;
    }

    private void OnDisable()
    {
        notificationsManager.OnNotificationsCanceled -= OnNotificationsCanceled;
    }

    protected abstract void ApplyNotifications();

    protected abstract bool ShouldSendNotification();

    protected abstract int GetFireTimeInSeconds();

    protected virtual string GetNotificationLabel()
    {
        if (!LabelLocalizationItem) return null;

        var localizationManager = LocalizationManager.Instance;
        if (localizationManager == null) {
            Debug.LogError("localizationManager is not valid");
            return null;
        }

        return localizationManager.GetText(LabelLocalizationItem);
    }

    protected virtual string GetNotificationBodyText()
    {
        if (!bodyLocalizationItem) return null;

        var localizationManager = LocalizationManager.Instance;
        if (localizationManager == null) {
            Debug.LogError("localizationManager is not valid");
            return null;
        }

        return localizationManager.GetText(bodyLocalizationItem);
    }

    protected virtual string GetNotificationSubtitleText()
    {
        if (!subtitleLocalizationItem) return null;

        var localizationManager = LocalizationManager.Instance;
        if (localizationManager == null) {
            Debug.LogError("localizationManager is not valid");
            return null;
        }

        return localizationManager.GetText(subtitleLocalizationItem);
    }

    private void OnNotificationsCanceled()
    {
        if (!ShouldSendNotification()) return;

        ApplyNotifications();
    }
}