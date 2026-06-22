using UnityEngine;

public class EnergyShortageNotificationController : NotificationController
{
    [SerializeField] private CityStorage cityStorage;

    private ItemInstance item;

    protected override bool TrySubscribe()
    {
        item = cityStorage.Inventory.GetItemById((int)ItemID.Electricity);
        if (item == null) return false;

        if (!base.TrySubscribe()) return false;

        item.OnAmountChanged += OnEnergyAmountChanged;
        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (item == null) return false;
        if (!base.TryUnsubscribe()) return false;

        item.OnAmountChanged -= OnEnergyAmountChanged;
        return true;
    }

    protected override bool ShouldCreateNotification()
    {
        return item.Amount <= 0;
    }

    private void OnEnergyAmountChanged(int amount)
    {
        UpdateNotificationCreated();
    }
}