using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceWidget : MonoBehaviour
{
    private CityStorage cityStorage;
    private EntitiesManager entitiesManager;

    [SerializeField] private ItemData itemData;
    private ItemInstance itemInstance;
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

        if (itemData.ItemId == (int)ItemID.Population) {
            EventBus.onCitizenInited += OnCitizenInited;
            EventBus.onCitizenDeleted -= OnCitizenDeleted;
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
        if (!TryToAssignItem()) return;

        UpdateAmount();
    }

    public void Init(ItemData itemData)
    {
        cityStorage = FindAnyObjectByType<CityStorage>();

        SetItem(itemData);
    }

    private bool TryToAssignItem()
    {
        if (itemInstance != null) return false;

        SetItem(itemData);
        return true;
    }

    private void SetItem(ItemData itemData)
    {
        int id = itemData.ItemId;
        this.itemData = itemData;
        itemInstance = cityStorage.Inventory.itemsDict[id].item;
        OnItemSet();
    }

    private void OnItemSet()
    {
        Sprite sprite = itemData.ItemIcon;
        SetImage(sprite);
    }

    private void UpdateAmount()
    {
        int populationId = (int)ItemID.Population;

        if (itemData.ItemId == populationId) {
            int id = populationId;
            int amount = entitiesManager.citizens.Count;
            int maxAmount = cityStorage.Inventory.itemsDict[id].maxAmount;
            SetAmount(amount, maxAmount);
        }
        else {
            if (itemInstance == null) return;

            int amount = itemInstance.Amount;
            int maxAmount = cityStorage.Inventory.itemsDict[itemInstance.ItemData.ItemId].maxAmount;
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
