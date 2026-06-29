using UnityEngine;

public static class NotificationFactory
{
    public static GameObject CreateNotification(GameObject notificationPrefab, Transform transform)
    {
        if (!notificationPrefab) {
            Debug.LogError("notificationPrefab is not valid");
            return null;
        }

        var notification = GameObject.Instantiate(notificationPrefab, transform);
        return notification;
    }
}