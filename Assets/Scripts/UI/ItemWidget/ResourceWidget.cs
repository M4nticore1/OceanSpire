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

public class ResourceWidget : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private ItemDefinition itemDefinition;
    public ItemDefinition ItemDefinition => itemDefinition;

    [SerializeField] private bool isCityItem = false;
    [SerializeField] private bool useLimit = false;

    public ItemInstance Item {  get; private set; }

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

    private CityStorage cityStorage => CityStorage.Instance;

    private void OnEnable()
    {
        if (infoButton) {
            infoButton.OnReleased.AddListener(OnInfoButtonClicked);
        }

        UpdateItemName();
        UpdateIcon();
        UpdateAmountAndLimit();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    private void OnDisable()
    {
        if (infoButton) {
            infoButton.OnReleased.RemoveListener(OnInfoButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (Amounts != null) {
            for (int i = Amounts.Count - 1; i >= 0; i--) {
                if (Amounts[i] != null) {
                    Amounts[i].OnAmountChanged -= OnAmountChanged;
                }
            }
            Amounts.Clear();
        }

        if (Limit != null) {
            Limit.OnAmountChanged -= OnLimitChanged;
            Limit = null;
        }
    }

    protected virtual void Start()
    {
        UpdateItemFromCityStorage();
        UpdateItemName();
        UpdateIcon();
        UpdateAmountAndLimit();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    public virtual void SetItem(ItemInstance itemInstance)
    {
        Item = itemInstance;
    }

    public virtual void SetItemDefinition(ItemDefinition definition)
    {
        itemDefinition = definition;

        UpdateItemFromCityStorage();
        UpdateItemName();
        UpdateIcon();
    }

    protected virtual void UpdateAmountAndLimit()
    {
        if (Amounts == null) return;

        int amountsSum = CalculateAmountsSum();
        if (useLimit && Limit != null) {
            SetAmountText(amountsSum, Limit.Amount);
        }
        else {
            SetAmountText(amountsSum);
        }
    }

    protected virtual int CalculateAmountsSum()
    {
        int sum = 0;
        foreach (var amount in Amounts) {
            sum += amount.Amount;
        }

        return sum;
    }

    public void AddAmount(IItemAmount amount)
    {
        if (Amounts.Contains(amount)) return;

        Amounts.Add(amount);
        amount.OnAmountChanged += OnAmountChanged;

        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    public void RemoveAmount(IItemAmount amount)
    {
        Amounts.Remove(amount);
        amount.OnAmountChanged -= OnAmountChanged;

        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    public void SetLimit(IItemAmount amount)
    {
        if (Limit != null) {
            Limit.OnAmountChanged -= OnLimitChanged;
        }

        Limit = amount;
        Limit.OnAmountChanged += OnLimitChanged;

        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    public void SetColor(Color color)
    {
        if (!resourceAmountText) return;

        resourceAmountText.color = color;
    }

    public void SetAmountText(int amount)
    {
        if (!resourceAmountText) return;

        resourceAmountText.SetText(amount.ToString());
    }

    public void SetAmountText(int amount, int limit)
    {
        if (!resourceAmountText) return;

        resourceAmountText.SetText(amount.ToString() + "/" + limit.ToString());
        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    public void SetIsCityItem(bool value)
    {
        isCityItem = value;
    }

    private void UpdateItemFromCityStorage()
    {
        if (!isCityItem) return;
        if (!cityStorage) return;
        if (!itemDefinition) return;

        SetItem(cityStorage.Inventory.GetItem(itemDefinition.ItemId));
    }

    private void UpdateIcon()
    {
        if (!itemDefinition) return;

        resourceImage.sprite = itemDefinition.ItemIcon;
    }

    private void TryUpdateResourceBar()
    {
        if (!resourceAmountBar) return;

        float alpha = 0;
        int amountsSum = CalculateAmountsSum();

        if (Limit != null && Limit.Amount > 0) {
            alpha = (float)amountsSum / Limit.Amount;
        }
        else {
            alpha = 0.0f;
        }

        resourceAmountBar.fillAmount = alpha;
    }

    private void UpdateItemName()
    {
        if (!itemNameText) return;
        if (!itemDefinition) return;

        itemNameText.SetLocalizationItem(itemDefinition.NameLocalizationItem);
    }

    private void TryUpdateAmountColor()
    {
        if (!useAmountColors) return;

        SetColor(IsEnough() ? enoughAmountColor : notEnoughAmountColor);
    }

    private void OnAmountChanged(int amount)
    {
        UpdateAmountAndLimit();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    private void OnLimitChanged(int amount)
    {
        UpdateAmountAndLimit();
    }

    private void OnInfoButtonClicked()
    {
        var informationMenu = ItemInformationMenu.Instance;
        if (!informationMenu) return;

        informationMenu.Show(Item);
    }

    private bool IsEnough()
    {
        if (changeColorType == ItemWidgetColorType.GreaterOrEqual) {
            return Amounts.Count <= 0 || Limit == null || CalculateAmountsSum() >= Limit.Amount;
        }
        else if (changeColorType == ItemWidgetColorType.Greater) {
            return Amounts.Count <= 0 || Limit == null || CalculateAmountsSum() > Limit.Amount;
        }
        else if (changeColorType == ItemWidgetColorType.Zero) {
            return Amounts.Count <= 0 || Limit == null || CalculateAmountsSum() > 0;
        }

        return false;
    }
}