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
            inventory.AddItemAmount(item.ItemId, 0);
        }
    }

    private void OnEnable()
    {
        inventory.OnItemAmountChanged += OnItemAmountChanged;
        inventory.OnItemLimitChanged += OnItemMaxAmountChanged;
    }

    private void OnDisable()
    {
        inventory.OnItemAmountChanged -= OnItemAmountChanged;
        inventory.OnItemLimitChanged -= OnItemMaxAmountChanged;
    }

    public void Init()
    {
        Init(InventoryData.Default() ?? new InventoryData());
    }

    public void Init(InventoryData inventoryData)
    {
        if (inventoryData == null) {
            Debug.LogError("[CityStorage] InventoryData is not valid");
            Init();
            return;
        }

        inventory.Init(inventoryData);
    }

    private void OnItemAmountChanged(ItemInstance item)
    {
        EventBus.InvokeMainStorageAmountChanged(item);
    }

    private void OnItemMaxAmountChanged(StorageItem item)
    {
        EventBus.InvokeMainStorageMaxAmountChanged(item);
    }
}
