using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceWidget : MonoBehaviour
{
    private CityStorage cityStorage;
    private EntitiesManager entitiesManager;

    [SerializeField] private ItemData itemData;
    private ItemInstance amountItem;
    private ItemInstance maxAmountItem;

    [SerializeField] private Image resourceImage;
    [SerializeField] private TextMeshProUGUI resourceAmountText;
    [SerializeField] private Image resourceAmountBar;

    private void Awake()
    {
        cityStorage = FindAnyObjectByType<CityStorage>();
        entitiesManager = FindAnyObjectByType<EntitiesManager>();
    }

    private void OnEnable()
    {
        EventBus.onMainStorageItemAmountChanged += OnLootAdded;
        EventBus.onItemRemoved += OnLootRemoved;
        EventBus.onStorageCapacityChanged += OnStorageCapacityChanged;

        if (itemData && itemData.ItemId == (int)ItemID.Population) {
            EventBus.onCitizenInited += OnCitizenInited;
            EventBus.onCitizenDeleted += OnCitizenDeleted;
        }

        UpdateAmount();
    }

    private void OnDisable()
    {
        EventBus.onMainStorageItemAmountChanged -= OnLootAdded;
        EventBus.onItemRemoved -= OnLootRemoved;
        EventBus.onStorageCapacityChanged -= OnStorageCapacityChanged;

        if (itemData.ItemId == (int)ItemID.Population) {
            EventBus.onCitizenInited -= OnCitizenInited;
            EventBus.onCitizenDeleted -= OnCitizenDeleted;
        }
    }

    private void Start()
    {
        //if (!TryToAssignItem()) return;

        TryToAssignItem();
        UpdateAmount();
    }

    public void Init(ItemInstance amountItem, ItemInstance maxAmountItem)
    {
        cityStorage = FindAnyObjectByType<CityStorage>();

        SetItem(amountItem, maxAmountItem);
    }

    public void Init(ItemInstance amountItem)
    {
        cityStorage = FindAnyObjectByType<CityStorage>();

        SetItem(amountItem);
    }

    private bool TryToAssignItem()
    {
        if (amountItem != null)
            return false;

        SetItem(itemData);
        return true;
    }

    private void SetItem(ItemInstance amountItem, ItemInstance maxAmountItem)
    {
        this.amountItem = amountItem;
        this.maxAmountItem = maxAmountItem;
        OnItemSet();
    }

    private void SetItem(ItemInstance amountItem)
    {
        this.amountItem = amountItem;
        OnItemSet();
    }

    private void SetItem(ItemData itemData)
    {
        int id = itemData.ItemId;
        amountItem = cityStorage.Inventory.itemsDict[id].item;
        OnItemSet();
    }

    private void OnItemSet()
    {
        itemData = amountItem.ItemData;
        Sprite sprite = itemData.ItemIcon;
        SetImage(sprite);
    }

    private void UpdateAmount()
    {
        if (!itemData)
            return;

        int populationId = (int)ItemID.Population;

        if (itemData.ItemId == populationId) {
            if (!cityStorage.Inventory.itemsDict.ContainsKey(populationId)) return;

            int id = populationId;
            int amount = entitiesManager.citizens.Count;
            int maxAmount = cityStorage.Inventory.itemsDict[id].maxAmount;
            SetAmount(amount, maxAmount);
        }
        else {
            if (amountItem == null) return;

            int id = amountItem.ItemData.ItemId;
            int amount = amountItem.Amount;
            int maxAmount = maxAmountItem != null ? maxAmountItem.Amount : cityStorage.Inventory.itemsDict[id].maxAmount;
            SetAmount(amount, maxAmount);
        }
    }

    public void SetAmount(int amount)
    {
        resourceAmountText.SetText(amount.ToString());
    }

    public void SetAmount(int amount, int maxAmount)
    {
        resourceAmountText.SetText(amount.ToString() + "/" + maxAmount.ToString());
        if (resourceAmountBar) {
            float alpha = 0;
            if (maxAmount > 0)
                alpha = (float)amount / maxAmount;
            else
                alpha = 0.0f;
            resourceAmountBar.fillAmount = alpha;
        }
    }

    public void SetImage(Sprite resourceSprite)
    {
        resourceImage.sprite = resourceSprite;
    }

    private void OnLootAdded(ItemInstance item)
    {
        if (item.ItemData.ItemId != itemData.ItemId) return;

        UpdateAmount();
    }

    private void OnLootRemoved(ItemInstance item)
    {
        if (item.ItemData.ItemId != itemData.ItemId) return;

        UpdateAmount();
    }

    private void OnStorageCapacityChanged()
    {
        UpdateAmount();
    }

    private void OnCitizenInited(Human citizen)
    {
        UpdateAmount();
    }

    private void OnCitizenDeleted(Human citizen)
    {
        UpdateAmount();
    }
}
