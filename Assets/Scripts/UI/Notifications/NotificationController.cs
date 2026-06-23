using UnityEngine;

public abstract class NotificationController : MonoBehaviour
{
    [SerializeField] private GameObject notificationPrefab;

    [SerializeField] private NotificationsPanel notificationsPanel;
    public NotificationsPanel NotificationsPanel => notificationsPanel;

    private GameObject spawnedNotification;
    private bool isSubscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void Start()
    {
        TrySubscribe();
        TryCreateNotification();
    }

    protected virtual bool TrySubscribe()
    {
        if (isSubscribed) return false;

        isSubscribed = true;
        return true;
    }

    protected virtual bool TryUnsubscribe()
    {
        if (!isSubscribed) return false;

        isSubscribed = false;
        return false;
    }

    protected abstract bool ShouldCreateNotification();

    protected void UpdateNotificationCreated()
    {
        if (ShouldCreateNotification()) {
            CreateNotification();
        }
        else {
            DestroyNotification();
        }
    }

    protected void TryCreateNotification()
    {
        if (!ShouldCreateNotification()) return;

        CreateNotification();
    }

    protected void CreateNotification()
    {
        if (spawnedNotification) return;

        spawnedNotification = NotificationFactory.CreateNotification(notificationPrefab, notificationsPanel.LayoutGroup.transform);
    }

    protected void DestroyNotification()
    {
        if (!spawnedNotification) return;

        Destroy(spawnedNotification);
    }
}