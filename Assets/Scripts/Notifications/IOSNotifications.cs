using System;
using UnityEngine;

#if UNITY_IOS
using Unity.Notifications.iOS;
using System.Collections;
#endif

public class IOSNotifications : MonoBehaviour
{
#if UNITY_IOS
    public IEnumerator RequestAuthorization()
    {
        using var request = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true);
        while (!request.IsFinished) {
            yield return null;
        }
    }

    public void SendNotification(string title, string body, string subtitle, int fireTimeInSeconds)
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(0, 0, fireTimeInSeconds),
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            Identifier = "lives_full",
            Title = title,
            Body = body,
            Subtitle = subtitle,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Badge),
            CategoryIdentifier = "default_category",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }
#endif
}