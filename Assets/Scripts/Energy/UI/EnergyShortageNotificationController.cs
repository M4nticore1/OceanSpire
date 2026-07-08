using UnityEngine;

public class EnergyShortageNotificationController : GameNotificationController
{
    public static EnergyShortageNotificationController Instance { get; private set; }

    [SerializeField] private CityStorage cityStorage;

    private ItemInstance item;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override bool TrySubscribe()
    {
        item = cityStorage.Inventory.GetItemById(ItemID.Electricity);
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

    protected override bool ShoulNotificate()
    {
        return item.Amount <= 0;
    }

    private void OnEnergyAmountChanged(int amount)
    {
        UpdateNotification();
    }
}