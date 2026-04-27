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
            CreaturesManager.onCitizenRegistered += OnCitizenAdded;
            CreaturesManager.onCitizenUnregistered += OnCitizenRemoved;
            EventBus.onCitizenRevived += OnCitizenRevived;
            EventBus.onCitizenDied += OnCitizenDied;
        }

        UpdateAmount();
    }

    private void OnDisable()
    {
        EventBus.onMainStorageItemAmountChanged -= OnMainStorageItemAmountChanged;
        EventBus.onMainStorageItemMaxAmountChanged -= OnMainStorageItemMaxAmountChanged;

        if (itemData && itemData.ItemId == (int)ItemID.Population) {

            CreaturesManager.onCitizenRegistered -= OnCitizenAdded;
            CreaturesManager.onCitizenUnregistered -= OnCitizenRemoved;
            EventBus.onCitizenRevived -= OnCitizenRevived;
            EventBus.onCitizenDied -= OnCitizenDied;
        }
    }

    private void Start()
    {
        UpdateAmount();
    }

    public void SetAmountItem(ItemInstance item)
    {
        amountItem = item;
        itemData = amountItem.ItemData;
        resourceImage.sprite = itemData.ItemIcon;
    }

    public void SetMaxAmountItem(ItemInstance item)
    {
        maxAmountItem = item;
    }

    public void SetColor(Color color)
    {
        resourceAmountText.color = color;
    }

    public void UpdateAmount()
    {
        TryApplyItemData();

        if (HasPopulationItem()) {
            UpdateCitizensAmount();
        }
        else {
            UpdateResourceAmount();
        }
    }

    private void SetAmountText(int amount)
    {
        resourceAmountText.SetText(amount.ToString());
    }

    private void SetAmountText(int amount, int maxAmount)
    {
        resourceAmountText.SetText(amount.ToString() + "/" + maxAmount.ToString());
        ApplyResourceBar();
        UpdateAmountColor();
    }

    private void ApplyResourceBar()
    {
        if (!resourceAmountBar) return;

        float alpha = 0;

        if (maxAmountItem.Amount > 0) {
            alpha = (float)amountItem.Amount / maxAmountItem.Amount;
        }
        else {
            alpha = 0.0f;
        }

        resourceAmountBar.fillAmount = alpha;
    }

    private bool TryApplyItemData()
    {
        if (amountItem != null) return false;
        if (!itemData) return false;

        int id = itemData.ItemId;
        if (!CityStorage.Instance.Inventory.itemsDict.ContainsKey(id)) return false;

        SetAmountItem(CityStorage.Instance.Inventory.itemsDict[id].item);
        SetMaxAmountItem(CityStorage.Instance.Inventory.itemsDict[id].maxAmountItem);

        return true;
    }

    private void UpdateResourceAmount()
    {
        if (amountItem == null) return;

        int amount = amountItem.Amount;

        if (maxAmountItem != null) {
            int maxAmount = maxAmountItem.Amount;
            SetAmountText(amount, maxAmount);
        }
        else {
            SetAmountText(amount);
        }
    }

    private void UpdateCitizensAmount()
    {
        if (!itemData) return;
        if (amountItem == null) return;

        int id = (int)ItemID.Population;
        int amount = 0;
        int maxAmount = CityStorage.Instance.Inventory.itemsDict[id].maxAmount;

        foreach (var citizen in CreaturesManager.instance.citizens) {
            if (!citizen.HealthComponent.isAlive) continue;

            amount++;
        }

        SetAmountText(amount, maxAmount);
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
        if (!itemData) return;
        if (item.ItemData.ItemId != itemData.ItemId) return;

        UpdateResourceAmount();
    }

    private void OnMainStorageItemMaxAmountChanged(StorageItem item)
    {
        if (item.item.ItemData.ItemId != itemData.ItemId) return;

        UpdateResourceAmount();
    }

    private void OnCitizenAdded(Human citizen)
    {
        UpdateCitizensAmount();
    }

    private void OnCitizenRemoved(Human citizen)
    {
        UpdateCitizensAmount();
    }

    private void OnCitizenRevived(Human human)
    {
        UpdateCitizensAmount();
    }

    private void OnCitizenDied(Human human)
    {
        UpdateCitizensAmount();
    }

    private bool HasPopulationItem()
    {
        if (!itemData) return false;

        int id = (int)ItemID.Population;
        return itemData.ItemId == id;
    }
}