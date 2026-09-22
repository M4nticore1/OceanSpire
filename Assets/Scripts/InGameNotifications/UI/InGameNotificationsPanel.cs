using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InGameNotificationsPanel : MonoBehaviour
{
    [SerializeField] private InGameNotificationWidget notificationWidgetPrefab;
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private SelectGroup selectGroup;

    private Dictionary<InGameNotificationData, InGameNotificationWidget> spawnedNotificationWidgetsDict = new();

    private void OnEnable()
    {
        InGameNotificationWidget.OnNotificationHidden += HandleNotificationWidgetHidden;
    }

    private void OnDisable()
    {
        InGameNotificationWidget.OnNotificationHidden -= HandleNotificationWidgetHidden;
    }

    public void ShowNotification(InGameNotificationData notificationData)
    {
        var widget = InGameNotificationFactory.CreateNotification(notificationWidgetPrefab, layoutGroup.transform, notificationData);
        if (widget == null) {
            Debug.LogError($"[{nameof(InGameNotificationsPanel)}] Created Widget is not valid from {notificationWidgetPrefab}!");
            return;
        }

        widget.SetButtonSelectGroup(selectGroup);
        spawnedNotificationWidgetsDict.Add(notificationData, widget);
    }

    public void HideNotification(InGameNotificationData notificationData)
    {
        if (notificationWidgetPrefab == null) {
            Debug.LogError($"[{nameof(InGameNotificationsPanel)}] Notification Prefab is not valid!");
            return;
        }

        spawnedNotificationWidgetsDict.TryGetValue(notificationData, out var widget);
        if (widget == null) return;

        spawnedNotificationWidgetsDict.Remove(notificationData);
        Destroy(widget.gameObject);
    }

    public InGameNotificationWidget GetNotificationWidget(InGameNotificationData notificationData)
    {
        spawnedNotificationWidgetsDict.TryGetValue(notificationData, out var widget);

        return widget;
    }

    private void HandleNotificationWidgetHidden(InGameNotificationWidget widget)
    {
        if (!widget) return;

        HideNotification(widget.NotificationData);
    }
}