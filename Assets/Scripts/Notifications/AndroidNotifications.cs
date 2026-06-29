using System;
using UnityEngine;

#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif

public class AndroidNotifications : MonoBehaviour
{
#if UNITY_ANDROID
    public void RequestAuthorization()
    {
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS")) {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
    }

    public void RegisterNotificationChannel()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = "default_channel",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Full Lives"
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    public void SendNotification(string title, string text, int fireTimeInSeconds)
    {
        var notification = new AndroidNotification()
        {
            Title = title,
            Text = text,
            FireTime = DateTime.Now.AddSeconds(fireTimeInSeconds),
            SmallIcon = "icon_0",
            LargeIcon = "icon_1"
        };

        AndroidNotificationCenter.SendNotification(notification, "default_channel");
    }
#endif
}