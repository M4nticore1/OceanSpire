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
    [SerializeField] private Image resourceImage;
    [SerializeField] private TextMeshProUGUI resourceAmountText;
    [SerializeField] private Image resourceAmountBar;

    [Header("Color")]
    [SerializeField] private bool useAmountColors = false;
    [SerializeField] private Color enoughAmountColor = Color.green;
    [SerializeField] private Color notEnoughAmountColor = Color.red;

    protected override void OnEnable()
    {
        base.OnEnable();

        UpdateAmount();
    }

    protected override void Start()
    {
        base.Start();

        UpdateAmountFromDefinition();
        UpdateAmount();
    }

    public virtual void SetItem(ItemDefinition definition)
    {
        itemDefinition = definition;
        resourceImage.sprite = itemDefinition.ItemIcon;
    }

    public void SetAmount(IItemAmount amount)
    {
        Amount = amount;
    }

    public void SetLimit(IItemAmount amount)
    {
        if (!useLimit) return;

        Limit = amount;
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
        TryUpdateResourceBar(amount, limit);
        TryUpdateAmountColor(amount, limit);
    }

    private void TryUpdateResourceBar(int amount, int limit)
    {
        if (!resourceAmountBar) return;

        float alpha = 0;

        if (limit > 0) {
            alpha = (float)amount / limit;
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

    private void UpdateAmount()
    {
        if (Amount == null) return;

        if (Limit != null) {
            SetAmountText(Amount.Amount, Limit.Amount);
        }
        else {
            SetAmountText(Amount.Amount);
        }
    }

    private void TryUpdateAmountColor(int amount, int limit)
    {
        if (!useAmountColors) return;

        if (amount >= limit) {
            SetColor(enoughAmountColor);
        }
        else {
            SetColor(notEnoughAmountColor);
        }
    }
}