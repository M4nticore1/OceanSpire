using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class StorageFullInGameNotificationController : InGameNotificationController, ILocalizable
{
    [Header("Storage")]
    [SerializeField] private CityStorageOverflowManager cityStorageOverflorManager;
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

        cityStorageOverflorManager.OnOverflowingStackAdded += HandleOverflowingStackAdded;
        cityStorageOverflorManager.OnOverflowingStackRemoved += HandleOverflowingStackRemoved;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        cityStorageOverflorManager.OnOverflowingStackAdded -= HandleOverflowingStackAdded;
        cityStorageOverflorManager.OnOverflowingStackRemoved -= HandleOverflowingStackRemoved;
    }

    public Dictionary<string, string> GetLocalization()
    {
        var stacks = cityStorageOverflorManager.OverflowingStacks.ToList();
        var sb = new StringBuilder();

        for (int i = 0; i < stacks.Count; i++) {
            var stack = stacks[i];
            if (stack == null) continue;

            if (excludedStacks.Contains(stack.Definition)) {
                stacks.RemoveAt(i);
                i--;
                continue;
            }

            var stackName = LocalizationManager.Instance.GetLocalizedText(stack.Definition.NameLocalizationItem);
            sb.Append($"<color=#FFC04D>{stackName}</color>");

            if (i < stacks.Count - 1) {
                sb.Append(", ");
            }
        }

        return new Dictionary<string, string>()
        {
            { "overflowingStacks", sb.ToString() },
            { "overflowingStacksCount", stacks.Count.ToString() }
        };
    }

    private void HandleOverflowingStackAdded(ItemStackInstance stack)
    {
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
        if (excludedStacks.Contains(stack.Definition)) return;

        var widget = GetNotificationWidget(notificationData);
        if (widget == null) return;

        widget.Init(notificationData);
    }
}