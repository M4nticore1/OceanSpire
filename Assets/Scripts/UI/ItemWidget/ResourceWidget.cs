using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ResourceWidget : UIBehaviour
{
    [Header("Main")]
    [SerializeField] private ItemDefinition itemDefinition;
    public ItemDefinition ItemDefinition => itemDefinition;

    [SerializeField] private bool useLimit = false;

    public IItemAmount Amount { get; private set; }
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
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (Amount != null)
            Amount.OnAmountChanged -= OnAmountChanged;

        if (Limit != null)
            Limit.OnAmountChanged -= OnLimitChanged;
    }

    protected override void Start()
    {
        base.Start();

        UpdateItemName();
        UpdateIcon();
        UpdateAmountFromDefinition();
        UpdateAmountAndLimit();
    }

    public virtual void SetItem(ItemDefinition definition)
    {
        itemDefinition = definition;
        UpdateItemName();
        UpdateIcon();
    }

    public void SetAmount(IItemAmount amount)
    {
        if (Amount != null) {
            Amount.OnAmountChanged -= OnAmountChanged;
        }

        Amount = amount;
        amount.OnAmountChanged += OnAmountChanged;

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
        resourceAmountText.color = color;
    }

    public void SetAmountText(int amount)
    {
        resourceAmountText.SetText(amount.ToString());
    }

    public void SetAmountText(int amount, int limit)
    {
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

        if (Limit != null && Limit.Amount > 0) {
            alpha = (float)Amount.Amount / Limit.Amount;
        }
        else {
            alpha = 0.0f;
        }

        resourceAmountBar.fillAmount = alpha;
    }

    private bool UpdateAmountFromDefinition()
    {
        if (Amount != null) return false;
        if (!itemDefinition) return false;

        int id = itemDefinition.ItemId;

        ItemInstance item = CityStorage.Instance.Inventory.GetItemById(id);
        if (item == null) return false;

        SetAmount(item);
        return true;
    }

    private void UpdateItemName()
    {
        if (!itemNameText) return;

        itemNameText.SetLocalizationItem(itemDefinition.NameLocalization);
    }

    private void UpdateAmountAndLimit()
    {
        if (Amount == null) return;

        if (Limit != null) {
            SetAmountText(Amount.Amount, Limit.Amount);
        }
        else {
            SetAmountText(Amount.Amount);
        }
    }

    private void TryUpdateAmountColor()
    {
        if (!useAmountColors) return;

        if (Amount == null || Limit == null || Amount.Amount >= Limit.Amount) {
            SetColor(enoughAmountColor);
        }
        else {
            SetColor(notEnoughAmountColor);
        }
    }

    private void OnAmountChanged()
    {
        UpdateAmountAndLimit();
        TryUpdateAmountColor();
    }

    private void OnLimitChanged()
    {
        UpdateAmountAndLimit();
    }
}