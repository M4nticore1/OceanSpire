using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceWidget : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    private ItemInstance itemInstance;
    [SerializeField] private Image resourceImage;
    [SerializeField] private TextMeshProUGUI resourceAmountText;
    [SerializeField] private Image resourceAmountBar;

    private void OnEnable()
    {
        EventBus.onMainStorageItemAmountChanged += OnLootAdded;
        EventBus.onItemRemoved += OnLootRemoved;
        EventBus.onStorageCapacityChanged += OnStorageCapacityChanged;

        UpdateAmount();
    }

    private void OnDisable()
    {
        EventBus.onMainStorageItemAmountChanged -= OnLootAdded;
        EventBus.onItemRemoved -= OnLootRemoved;
        EventBus.onStorageCapacityChanged -= OnStorageCapacityChanged;
    }

    private void Start()
    {
        if (itemInstance != null) return;

        SetItem(itemData);
        UpdateAmount();
    }

    private void OnLootAdded(ItemInstance item)
    {
        if (item.ItemData.ItemId != itemData.ItemId) return;
        Debug.Log(item.ItemData.ItemName);
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

    public void SetItem(ItemData itemData)
    {
        int id = itemData.ItemId;
        itemInstance = CityManager.Instance.Inventory.items[id].item;
        OnItemSet();
    }

    public void SetItem(ItemInstance item)
    {
        itemInstance = item;
        itemData = itemInstance.ItemData;
        OnItemSet();
    }

    private void OnItemSet()
    {
        Sprite sprite = itemData.ItemIcon;
        SetImage(sprite);
    }

    public void SetAmount(int amount)
    {
        resourceAmountText.SetText(amount.ToString());
    }

    public void SetAmount(int amount, int maxAmount)
    {
        resourceAmountText.SetText(amount.ToString() + "/" + maxAmount.ToString());
        if (resourceAmountBar)  {
            float alpha = 0;
            if (maxAmount > 0)
                alpha = (float)amount / maxAmount;
            else
                alpha = 0.0f;
            resourceAmountBar.fillAmount = alpha;
        }
    }

    private void UpdateAmount()
    {
        if (itemInstance == null) return;
        if (CityManager.Instance.Inventory.items.Count <= itemInstance.ItemData.ItemId) return;

        int amount = itemInstance.Amount;
        int maxAmount = CityManager.Instance.Inventory.items[itemInstance.ItemData.ItemId].maxAmount;
        SetAmount(amount, maxAmount);
    }

    public void SetImage(Sprite resourceSprite)
    {
        resourceImage.sprite = resourceSprite;
    }
}
