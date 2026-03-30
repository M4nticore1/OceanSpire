using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceWidget : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    private ItemInstance amountItem;
    private ItemInstance maxAmountItem;

    [SerializeField] private bool useCityStorage = false;

    [SerializeField] private Image resourceImage;
    [SerializeField] private TextMeshProUGUI resourceAmountText;
    [SerializeField] private Image resourceAmountBar;

    [SerializeField] private bool useAmountColors = false;
    [SerializeField] private Color enoughAmountColor = Color.green;
    [SerializeField] private Color notEnoughAmountColor = Color.red;

    private void OnEnable()
    {
        EventBus.onMainStorageItemAmountChanged += OnMainStorageItemAmountChanged;
        EventBus.onMainStorageItemMaxAmountChanged += OnMainStorageItemMaxAmountChanged;

        if (itemData && itemData.ItemId == (int)ItemID.Population) {
            CreaturesManager.onCitizenAdded += OnCitizenAdded;
            CreaturesManager.onCitizenRemoved += OnCitizenRemoved;
        }

        UpdateAmount();
    }

    private void OnDisable()
    {
        EventBus.onMainStorageItemAmountChanged -= OnMainStorageItemAmountChanged;
        EventBus.onMainStorageItemMaxAmountChanged -= OnMainStorageItemMaxAmountChanged;

        if (itemData.ItemId == (int)ItemID.Population) {
            CreaturesManager.onCitizenAdded -= OnCitizenAdded;
            CreaturesManager.onCitizenRemoved -= OnCitizenRemoved;
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
        SetItem(amountItem, maxAmountItem);
    }

    public void Init(ItemInstance amountItem)
    {
        SetItem(amountItem);
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

        UpdateAmountColor();
    }

    public void SetImage(Sprite resourceSprite)
    {
        resourceImage.sprite = resourceSprite;
    }

    private void SetColor(Color color)
    {
        resourceAmountText.color = color;
    }

    private bool TryToAssignItem()
    {
        if (amountItem != null) return false;
        if (maxAmountItem != null) return false;

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
        amountItem = CityStorage.instance.Inventory.itemsDict[id].item;
        OnItemSet();
    }

    private void OnItemSet()
    {
        itemData = amountItem.ItemData;

        if (useCityStorage && maxAmountItem == null) {
            int id = itemData.ItemId;
            maxAmountItem = CityStorage.instance.Inventory.itemsDict[id].maxAmountItem;
        }

        Sprite sprite = itemData.ItemIcon;
        SetImage(sprite);
    }

    private void UpdateAmount()
    {
        if (!itemData) return;

        int populationId = (int)ItemID.Population;

        if (itemData.ItemId == populationId) {
            if (!CityStorage.instance.Inventory.itemsDict.ContainsKey(populationId))
                return;

            int id = populationId;
            int amount = CreaturesManager.instance.citizens.Count;
            int maxAmount = CityStorage.instance.Inventory.itemsDict[id].maxAmount;
            SetAmount(amount, maxAmount);
        }
        else {
            if (amountItem == null)
                return;

            int id = amountItem.ItemData.ItemId;
            int amount = amountItem.Amount;
            if (maxAmountItem != null){
                int maxAmount = maxAmountItem != null ? maxAmountItem.Amount : CityStorage.instance.Inventory.itemsDict[id].maxAmount;
                SetAmount(amount, maxAmount);
            }
            else {
                SetAmount(amount);
            }
        }
    }

    private void UpdateAmountColor()
    {
        if (!useAmountColors) return;

        if (amountItem.Amount >= maxAmountItem.Amount) {
            SetColor(enoughAmountColor);
        }
        else {
            SetColor(notEnoughAmountColor);
        }
    }

    private void OnMainStorageItemAmountChanged(ItemInstance item)
    {
        if (item.ItemData.ItemId != itemData.ItemId) return;

        UpdateAmount();
    }

    private void OnMainStorageItemMaxAmountChanged(StorageItem item)
    {
        if (item.item.ItemData.ItemId != itemData.ItemId) return;

        UpdateAmount();
    }

    private void OnCitizenAdded(Human citizen)
    {
        UpdateAmount();
    }

    private void OnCitizenRemoved(Human citizen)
    {
        UpdateAmount();
    }
}
