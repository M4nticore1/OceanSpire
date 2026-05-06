using UnityEngine;

public class CityStorage : MonoBehaviour
{
    public static CityStorage Instance { get; private set; }

    [SerializeField] private Inventory inventory;
    public Inventory Inventory => inventory;

    private void Awake()
    {
        Instance = this;

        foreach (var item in ItemsList.Instance.Items) {
            inventory.AddItem(item.ItemId, 0);
        }
    }

    private void OnEnable()
    {
        inventory.onItemAmountChanged += OnItemAmountChanged;
        inventory.onChangedItemMaxAmount += OnItemMaxAmountChanged;
        EventBus.onBoatUnloadedItem += OnBoatUnloadedItem;
    }

    private void OnDisable()
    {
        inventory.onItemAmountChanged -= OnItemAmountChanged;
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
        inventory.AddItem(id, amount);
    }
}
