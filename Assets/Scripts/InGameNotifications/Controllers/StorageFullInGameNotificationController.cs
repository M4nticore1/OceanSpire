using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class StorageFullInGameNotificationController : InGameNotificationController, ILocalizable
{
    [Header("Storage")]
    [SerializeField] private CityStorageOverflowManager cityStorageOverflowManager;
    [SerializeField] private List<ItemStackDefinition> excludedStacks = new();

    private InGameNotificationData notificationData;

    protected override void Awake()
    {
        base.Awake();

        notificationData = new InGameNotificationData(NameLocalizationItem, DescriptionLocalizationItem, NotificationIcon, SeverityDefinition, Priority, this, this);
    }

    protected override void Subscribe()
    {
        base.Subscribe();

        cityStorageOverflowManager.OnOverflowingStackAdded += HandleOverflowingStackAdded;
        cityStorageOverflowManager.OnOverflowingStackRemoved += HandleOverflowingStackRemoved;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        cityStorageOverflowManager.OnOverflowingStackAdded -= HandleOverflowingStackAdded;
        cityStorageOverflowManager.OnOverflowingStackRemoved -= HandleOverflowingStackRemoved;
    }

    public Dictionary<string, string> GetLocalization()
    {
        var validStacks = cityStorageOverflowManager.OverflowingStacks.Where(s => s != null && !excludedStacks.Contains(s.Definition)).ToList();

        var sb = new StringBuilder();
        var count = validStacks.Count;

        for (int i = 0; i < count; i++) {
            var stack = validStacks[i];
            var stackName = LocalizationManager.Instance.GetLocalizedText(stack.Definition.NameLocalizationItem);

            sb.Append($"<color=#FFC04D>{stackName}</color>");

            if (i < count - 1) {
                sb.Append(", ");
            }
        }

        return new Dictionary<string, string>()
        {
            { "overflowingStacks", sb.ToString() },
            { "overflowingStacksCount", count.ToString() }
        };
    }

    private void HandleOverflowingStackAdded(ItemStackInstance stack)
    {
        if (stack == null) return;
        if (excludedStacks.Contains(stack.Definition)) return;

        var widget = GetNotificationWidget(notificationData);

        if (widget != null) {
            widget.Init(notificationData);
        }
        else {
            ShowNotification(notificationData);
        }
    }

    private void HandleOverflowingStackRemoved(ItemStackInstance stack)
    {
        if (stack == null) return;
        if (excludedStacks.Contains(stack.Definition)) return;

        var widget = GetNotificationWidget(notificationData);
        if (widget == null) return;

        if (cityStorageOverflowManager.OverflowingStacks.Count > 0) {
            widget.Init(notificationData);
        }
        else {
            HideNotification(notificationData);
        }
    }
}