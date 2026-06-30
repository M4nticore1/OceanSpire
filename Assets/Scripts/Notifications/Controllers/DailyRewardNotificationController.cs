using System;
using UnityEngine;

public class DailyRewardNotificationController : NotificationController
{
    [Header("Daily Reward")]
    [SerializeField] private DailyRewardManager dailyRewardManager;
    [SerializeField] private int notificationTimeInHours = 12;

    [SerializeField] private int[] notificationDays;

    protected override bool ShouldSendNotification()
    {
        return true;
    }

    protected override void ApplyNotifications()
    {
        foreach (var day in notificationDays) {
            if (day == 0 && dailyRewardManager.ExtraRewardCollected) continue;

            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var targetTime = GetRewardResetDate().AddHours(notificationTimeInHours).ToUnixTimeSeconds() + day * 86400;

            if (targetTime < currentTime) {
                Debug.LogError("Target daily reward notification time is less than current time");
                continue;
            }

            var fireTime = (int)(targetTime - currentTime);

            NotificationsManager.SendNotification(GetLabelText(),GetBodyText(), GetSubtitleText(), fireTime);
        }
    }

    protected override int GetFireTimeInSeconds()
    {
        return 0;
    }

    private DateTimeOffset GetRewardResetDate()
    {
        long realResetTime = dailyRewardManager.NextResetTime;
        DateTimeOffset resetTimeDay = DateTimeOffset.FromUnixTimeSeconds(realResetTime);
        DateTimeOffset notificationResetDate = resetTimeDay.Date;

        return notificationResetDate;
    }
}