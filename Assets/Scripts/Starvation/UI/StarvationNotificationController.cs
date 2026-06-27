using UnityEngine;

public class StarvationNotificationController : NotificationController
{
    [SerializeField] private CityStorage cityStorage;

    private ItemInstance item;

    protected override bool TrySubscribe()
    {
        item = cityStorage.Inventory.GetItemById((int)ItemID.Food);
        if (item == null) return false;

        if (!base.TrySubscribe()) return false;

        item.OnAmountChanged += OnFoodAmountChanged;
        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (item == null) return false;
        if (!base.TryUnsubscribe()) return false;

        item.OnAmountChanged -= OnFoodAmountChanged;
        return true;
    }

    protected override bool ShoulNotificate()
    {
        return item.Amount <= 0;
    }

    private void OnFoodAmountChanged(int amount)
    {
        UpdateNotification();
    }
}