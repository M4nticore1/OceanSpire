using System.Collections.Generic;
using UnityEngine;

public class StorageFullInGameNotificationController : InGameNotificationController
{
    [Header("Storage")]
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private List<ItemStackDefinition> excludedStacks = new();

    private Dictionary<ItemStackInstance, InGameNotificationData> notificationsDict = new();
    private ItemStackInstance lastChangedStackItemAmount;

    protected override void Subscribe()
    {
        base.Subscribe();

        cityStorage.Inventory.OnStackItemAmountChanged += HandleStackItemChanged;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        cityStorage.Inventory.OnStackItemAmountChanged -= HandleStackItemChanged;
    }

    protected override InGameNotificationData GetNotificationData()
    {
        return new InGameNotificationData(NameLocalizationItem, DescriptionLocalizationItem, lastChangedStackItemAmount.Definition.Icon, SeverityDefinition, Priority, lastChangedStackItemAmount, lastChangedStackItemAmount);
    }

    private void HandleStackItemChanged(ItemStackInstance stack)
    {
        if (stack == null) return;
        if (excludedStacks.Contains(stack.Definition)) return;

        lastChangedStackItemAmount = stack;

        if (stack.IsOverflowed) {
            TryShowNotification();
        }
        else {
            TryHideNotification();
        }
    }

    private void TryShowNotification()
    {
        if (notificationsDict.ContainsKey(lastChangedStackItemAmount)) return;

        var data = GetNotificationData();
        notificationsDict.Add(lastChangedStackItemAmount, data);

        ShowNotification(data);
    }

    private void TryHideNotification()
    {
        notificationsDict.TryGetValue(lastChangedStackItemAmount, out var data);
        if (data == null) return;

        notificationsDict.Remove(lastChangedStackItemAmount);
        HideNotification(data);
    }
}