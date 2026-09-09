using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public enum ItemWidgetColorType
{
    GreaterOrEqual,
    Greater,
    Zero
}

public class ItemWidget : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private ItemDefinition itemDefinition;
    public ItemDefinition ItemDefinition => itemDefinition;

    [SerializeField] private bool isCityItem = false;
    [SerializeField] private bool useLimit = false;
    [SerializeField] private bool zeroAmountDestroy = false;

    public ItemInstance Item { get; private set; }

    public List<IItemAmount> Amounts { get; private set; } = new();
    public IItemAmount Limit { get; private set; }

    [Header("UI")]
    [SerializeField] private CustomButton infoButton;
    [SerializeField] private TextLocalizer itemNameText;
    [SerializeField] private TextMeshProUGUI resourceAmountText;
    [SerializeField] private Image resourceImage;
    [SerializeField] private Image resourceAmountBar;

    [Header("Color")]
    [SerializeField] private ItemWidgetColorType changeColorType = ItemWidgetColorType.Greater;
    [SerializeField] private bool useAmountColors = false;
    [SerializeField] private Color enoughAmountColor = Color.green;
    [SerializeField] private Color notEnoughAmountColor = Color.red;

    protected CityStorage cityStorage => CityStorage.Instance;

    protected virtual void OnEnable()
    {
        Subscribe();

        if (infoButton != null)
            infoButton.OnReleased.AddListener(OnInfoButtonClicked);

        UpdateDisplay();
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();

        if (infoButton != null)
            infoButton.OnReleased.RemoveListener(OnInfoButtonClicked);
    }

    protected virtual void Start()
    {
        UpdateItemFromCityStorage();
        UpdateDisplay();
    }

    // Subscription
    private void Subscribe()
    {
        foreach (var amount in Amounts) {
            if (amount == null)
                continue;

            amount.OnAmountChanged -= OnAmountChanged;
            amount.OnAmountChanged += OnAmountChanged;
        }

        if (Limit != null) {
            Limit.OnAmountChanged -= OnLimitChanged;
            Limit.OnAmountChanged += OnLimitChanged;
        }
    }

    private void Unsubscribe()
    {
        foreach (var amount in Amounts) {
            if (amount == null)
                continue;

            amount.OnAmountChanged -= OnAmountChanged;
        }

        if (Limit != null)
            Limit.OnAmountChanged -= OnLimitChanged;
    }

    // Item
    public virtual void SetItem(ItemInstance itemInstance)
    {
        if (itemInstance == null) {
            Debug.LogError(
                $"[{nameof(ItemWidget)}] Item Instance is not valid!"
            );
            return;
        }

        Item = itemInstance;

        SetItemDefinition(itemInstance.Definition);
    }

    public virtual void SetItemDefinition(ItemDefinition definition)
    {
        if (definition == null) {
            Debug.LogError(
                $"[{nameof(ItemWidget)}] Item Definition is not valid!"
            );
            return;
        }

        if (itemDefinition == definition)
            return;

        itemDefinition = definition;

        UpdateItemFromCityStorage();
        UpdateItemName();
        UpdateIcon();
    }

    private void UpdateItemFromCityStorage()
    {
        if (!isCityItem)
            return;

        if (cityStorage == null)
            return;

        if (itemDefinition == null)
            return;

        var item = cityStorage.Inventory.GetInventoryItem(
            itemDefinition.ItemId
        );

        if (item != null)
            SetItem(item);
    }

    // Amounts
    public void AddAmount(IItemAmount amount)
    {
        if (amount == null)
            return;

        if (Amounts.Contains(amount))
            return;

        Amounts.Add(amount);

        if (isActiveAndEnabled) {
            amount.OnAmountChanged -= OnAmountChanged;
            amount.OnAmountChanged += OnAmountChanged;
        }

        UpdateDisplay();
    }

    public void RemoveAmount(IItemAmount amount)
    {
        if (amount == null)
            return;

        if (!Amounts.Remove(amount))
            return;

        amount.OnAmountChanged -= OnAmountChanged;

        UpdateDisplay();
    }

    public void SetLimit(IItemAmount amount)
    {
        if (Limit == amount)
            return;

        if (Limit != null)
            Limit.OnAmountChanged -= OnLimitChanged;

        Limit = amount;

        if (isActiveAndEnabled && Limit != null) {
            Limit.OnAmountChanged -= OnLimitChanged;
            Limit.OnAmountChanged += OnLimitChanged;
        }

        UpdateDisplay();
    }

    protected virtual int CalculateAmountsSum()
    {
        int sum = 0;

        Debug.Log(this + " " + Amounts.Count);
        foreach (var amountable in Amounts) {
            if (amountable == null)
                continue;

            Debug.Log(amountable.Amount);
            sum += amountable.Amount;
        }

        return sum;
    }

    // Display
    private void UpdateDisplay()
    {
        UpdateAmountAndLimitText();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
        UpdateItemName();
        UpdateIcon();
    }

    protected void UpdateAmountAndLimitText()
    {
        int amount = CalculateAmountsSum();

        if (useLimit && Limit != null)
            SetAmountText(amount, Limit.Amount);
        else
            SetAmountText(amount);
    }

    private void TryUpdateResourceBar()
    {
        if (resourceAmountBar == null)
            return;

        if (Limit == null || Limit.Amount <= 0) {
            resourceAmountBar.fillAmount = 0f;
            return;
        }

        float fillAmount =
            (float)CalculateAmountsSum() / Limit.Amount;

        resourceAmountBar.fillAmount = Mathf.Clamp01(fillAmount);
    }

    private void TryUpdateAmountColor()
    {
        if (!useAmountColors)
            return;

        SetColor(
            IsEnough()
                ? enoughAmountColor
                : notEnoughAmountColor
        );
    }

    private void UpdateItemName()
    {
        if (itemNameText == null)
            return;

        if (itemDefinition == null)
            return;

        itemNameText.SetLocalizationItem(
            itemDefinition.NameLocalizationItem
        );
    }

    private void UpdateIcon()
    {
        if (resourceImage == null)
            return;

        if (itemDefinition == null)
            return;

        resourceImage.sprite = itemDefinition.ItemIcon;
    }

    // UI
    public void SetColor(Color color)
    {
        if (resourceAmountText == null)
            return;

        resourceAmountText.color = color;
    }

    public void SetAmountText(int amount)
    {
        if (resourceAmountText == null)
            return;

        resourceAmountText.SetText(amount.ToString());
    }

    public void SetAmountText(int amount, int limit)
    {
        if (resourceAmountText == null)
            return;

        resourceAmountText.SetText(
            $"{amount}/{limit}"
        );
    }

    public void SetIsCityItem(bool value)
    {
        isCityItem = value;
    }

    // Events
    private void OnAmountChanged(int amount)
    {
        if (zeroAmountDestroy && TryDestroy())
            return;

        UpdateAmountAndLimitText();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    private void OnLimitChanged(int amount)
    {
        UpdateAmountAndLimitText();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    private void OnInfoButtonClicked()
    {
        var informationMenu = ItemInformationMenu.Instance;

        if (informationMenu == null)
            return;

        informationMenu.Show(Item);
    }

    // Destroy
    private bool TryDestroy()
    {
        return Item != null && Item.Amount <= 0;
    }

    // Color condition
    private bool IsEnough()
    {
        int amount = CalculateAmountsSum();

        if (Amounts.Count <= 0 || Limit == null)
            return true;

        switch (changeColorType) {
            case ItemWidgetColorType.GreaterOrEqual:
                return amount >= Limit.Amount;

            case ItemWidgetColorType.Greater:
                return amount > Limit.Amount;

            case ItemWidgetColorType.Zero:
                return amount > 0;

            default:
                return false;
        }
    }
}