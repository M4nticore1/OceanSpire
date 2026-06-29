using UnityEngine;
using System;


#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class NotificationsManager : MonoBehaviour
{
    [SerializeField] private AndroidNotifications androidNotifications;
    [SerializeField] private IOSNotifications iosNotifications;

    public event Action OnNotificationsCanceled;

    private void Start()
    {
#if UNITY_ANDROID
        androidNotifications.RequestAuthorization();
        androidNotifications.RegisterNotificationChannel();
#endif

#if UNITY_IOS
        StartCoroutine(iosNotifications.RequestAuthorization());
#endif
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus) {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllNotifications();
#endif

#if UNITY_IOS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif

            OnNotificationsCanceled?.Invoke();
        }
    }

    public void SendNotification(string title, string body, string subtitle, int fireTimeIsSecond)
    {
#if UNITY_ANDROID
        androidNotifications.SendNotification(title, body, fireTimeIsSecond);
#endif

#if UNITY_IOS
        iosNotifications.SendNotification(title, body, subtitle, fireTimeIsSecond);
#endif
    }
}