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
        EventBus.onItemAdded += OnLootAdded;
        EventBus.onItemRemoved += OnLootRemoved;
        EventBus.onLootStorageChanged += OnStorageCapacityChanged;

        UpdateAmount();
    }

    private void OnDisable()
    {
        EventBus.onItemAdded -= OnLootAdded;
        EventBus.onItemRemoved -= OnLootRemoved;
        EventBus.onLootStorageChanged -= OnStorageCapacityChanged;
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

    private void OnStorageCapacityChanged(ItemInstance item)
    {
        if (item.ItemData.ItemId != itemData.ItemId) return;

        UpdateAmount();
    }

    public void SetItem(ItemData itemData)
    {
        int id = itemData.ItemId;
        itemInstance = CityManager.Instance.items[id];
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
        Debug.Log(1);
        if (itemInstance == null) return;
        if (CityManager.Instance.maxItemsAmount.Length <= itemInstance.ItemData.ItemId) return;
        Debug.Log(2);

        int amount = itemInstance.Amount;
        int maxAmount = CityManager.Instance.maxItemsAmount[itemInstance.ItemData.ItemId];
        SetAmount(amount, maxAmount);
        Debug.Log(amount);
    }

    public void SetImage(Sprite resourceSprite)
    {
        resourceImage.sprite = resourceSprite;
    }
}
