using System;
using UnityEngine;

public abstract class NotificationController : MonoBehaviour
{
    [SerializeField] private GameObject notificationPrefab;

    [SerializeField] private NotificationsPanel notificationsPanel;
    public NotificationsPanel NotificationsPanel => notificationsPanel;

    private GameObject spawnedNotification;
    private bool isSubscribed;

    public bool IsNotificated { get; private set; } = false;

    public event Action OnNotificated;
    public event Action OnUnnotificated;

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

    protected abstract bool ShoulNotificate();

    protected void UpdateNotification()
    {
        if (ShoulNotificate()) {
            Notificate();
            //CreateNotification();
        }
        else {
            Unnotificate();
            //DestroyNotification();
        }
    }

    protected void TryCreateNotification()
    {
        //if (!ShouldCreateNotification()) return;

        //CreateNotification();
    }

    protected void Notificate()
    {
        IsNotificated = true;
        OnNotificated?.Invoke();
        //if (spawnedNotification) return;

        //spawnedNotification = NotificationFactory.CreateNotification(notificationPrefab, notificationsPanel.LayoutGroup.transform);
    }

    protected void Unnotificate()
    {
        IsNotificated = false;
        OnUnnotificated?.Invoke();
        //if (!spawnedNotification) return;

        //Destroy(spawnedNotification);
    }
}