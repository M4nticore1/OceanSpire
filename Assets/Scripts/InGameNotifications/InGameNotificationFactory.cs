using UnityEngine;

public static class InGameNotificationFactory
{
    public static InGameNotificationWidget CreateNotification(InGameNotificationWidget notificationPrefab, Transform transform)
    {
        if (notificationPrefab == null) {
            Debug.LogError($"[{nameof(InGameNotificationFactory)}] Notification Prefab is not valid");
            return null;
        }

        var notification = GameObject.Instantiate(notificationPrefab, transform);
        return notification;
    }
}