using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameNotificationsPanel : MonoBehaviour
{
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private SelectGroup selectGroup;

    private Dictionary<InGameNotificationId, InGameNotificationWidget> spawnedNotificationWidgetsDict = new();

    public void ShowNotification(InGameNotificationWidget notificationPrefab)
    {
        if (notificationPrefab == null) {
            Debug.LogError($"[{nameof(InGameNotificationsPanel)}] Notification Prefab is not valid!");
            return;
        }

        var notificationId = notificationPrefab.NotificationId;
        if (spawnedNotificationWidgetsDict.ContainsKey(notificationId)) return;

        var widget = InGameNotificationFactory.CreateNotification(notificationPrefab, layoutGroup.transform);
        if (widget == null) {
            Debug.LogError($"[{nameof(InGameNotificationsPanel)}] Widget is not valid!");
            return;
        }

        widget.SetButtonSelectGroup(selectGroup);
        spawnedNotificationWidgetsDict.Add(notificationPrefab.NotificationId, widget);
    }

    public void HideNotification(InGameNotificationWidget notificationPrefab)
    {
        if (notificationPrefab == null) {
            Debug.LogError($"[{nameof(InGameNotificationsPanel)}] Notification Prefab is not valid!");
            return;
        }

        spawnedNotificationWidgetsDict.TryGetValue(notificationPrefab.NotificationId, out var widget);
        if (widget == null) return;

        spawnedNotificationWidgetsDict.Remove(notificationPrefab.NotificationId);
        Destroy(widget.gameObject);
    }
}