using UnityEngine;

public class CityStorage : MonoBehaviour
{
    public static CityStorage instance { get; private set; }

    [SerializeField] private Inventory inventory = null;
    public Inventory Inventory => GetInventory();

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        inventory.onChangedItemAmount += OnItemAmountChanged;
        inventory.onChangedItemMaxAmount += OnItemMaxAmountChanged;
        EventBus.onBoatUnloadedItem += OnBoatUnloadedItem;
    }

    private void OnDisable()
    {
        inventory.onChangedItemAmount -= OnItemAmountChanged;
        inventory.onChangedItemMaxAmount -= OnItemMaxAmountChanged;
        EventBus.onBoatUnloadedItem -= OnBoatUnloadedItem;
    }

    // Inventory
    private void OnItemAmountChanged(ItemInstance item)
    {
        EventBus.InvokeMainStorageAmountChanged(item);
    }

    private void OnItemMaxAmountChanged(StorageItem item)
    {
        EventBus.InvokeMainStorageMaxAmountChanged(item);
    }

    private void OnBoatUnloadedItem(int id, int amount)
    {
        inventory.AddItemAmount(id, amount);
    }

    private Inventory GetInventory()
    {
        return inventory;
    }
}
