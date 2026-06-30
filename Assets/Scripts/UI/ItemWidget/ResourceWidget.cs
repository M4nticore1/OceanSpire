using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public enum ItemWidgetColorType
{
    GreaterOrEqual,
    Greater,
    Zero
}

public class ResourceWidget : UIBehaviour
{
    [Header("Main")]
    [SerializeField] private ItemDefinition itemDefinition;
    public ItemDefinition ItemDefinition => itemDefinition;

    [SerializeField] private bool useLimit = false;

    public List<IItemAmount> Amounts { get; private set; } = new();
    public IItemAmount Limit { get; private set; }

    [Header("UI")]
    [SerializeField] private TextLocalizer itemNameText;
    [SerializeField] private TextMeshProUGUI resourceAmountText;
    [SerializeField] private Image resourceImage;
    [SerializeField] private Image resourceAmountBar;

    [Header("Color")]
    [SerializeField] private ItemWidgetColorType changeColorType = ItemWidgetColorType.Greater;
    [SerializeField] private bool useAmountColors = false;
    [SerializeField] private Color enoughAmountColor = Color.green;
    [SerializeField] private Color notEnoughAmountColor = Color.red;

    protected override void OnEnable()
    {
        base.OnEnable();

        UpdateItemName();
        UpdateIcon();
        UpdateAmountAndLimit();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach (var amount in Amounts) {
            amount.OnAmountChanged -= OnAmountChanged;
        }

        if (Limit != null)
            Limit.OnAmountChanged -= OnLimitChanged;
    }

    protected override void Start()
    {
        base.Start();

        UpdateItemName();
        UpdateIcon();
        UpdateAmountAndLimit();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    public virtual void SetItem(ItemDefinition definition)
    {
        itemDefinition = definition;
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

        itemNameText.SetLocalizationItem(itemDefinition.NameLocalization);
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