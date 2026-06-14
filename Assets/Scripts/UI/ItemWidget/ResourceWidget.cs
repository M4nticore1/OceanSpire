using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
        if (!useLimit) return;

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
        itemNameText.UpdateText();
    }

    protected virtual void UpdateAmountAndLimit()
    {
        if (Amounts == null) return;

        int amountsSum = CalculateAmountsSum();
        if (Limit != null) {
            SetAmountText(amountsSum, Limit.Amount);
        }
        else {
            SetAmountText(amountsSum);
        }
    }

    private void TryUpdateAmountColor()
    {
        if (!useAmountColors) return;

        int amountsSum = CalculateAmountsSum();
        if (Amounts == null || Limit == null || amountsSum >= Limit.Amount) {
            SetColor(enoughAmountColor);
        }
        else {
            SetColor(notEnoughAmountColor);
        }
    }

    private int CalculateAmountsSum()
    {
        int sum = 0;
        foreach (var amount in Amounts) {
            sum += amount.Amount;
        }

        return sum;
    }

    private void OnAmountChanged()
    {
        UpdateAmountAndLimit();
        TryUpdateResourceBar();
        TryUpdateAmountColor();
    }

    private void OnLimitChanged()
    {
        UpdateAmountAndLimit();
    }
}