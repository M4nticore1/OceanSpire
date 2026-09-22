using UnityEngine;

public enum ItemWidgetColorType
{
    GreaterOrEqual,
    Greater,
    Zero
}

public class ItemWidget : AmountableWidget
{
    [Header("Item Widget")]
    [SerializeField] private ItemDefinition itemDefinition;
    public ItemDefinition ItemDefinition => itemDefinition;

    public ItemInstance ItemInstance { get; private set; }

    [Header("Parameters")]
    [SerializeField] private bool isCityItem = false;

    protected CityStorage cityStorage => CityStorage.Instance;

    protected override void Start()
    {
        base.Start();

        UpdateItemFromCityStorage();
    }

    protected override void HandleInfoButtonClicked()
    {
        var informationMenu = InformationMenu.Instance;

        if (informationMenu == null)
            return;

        informationMenu.Show(ItemInstance);
    }

    protected override bool ShouldDestroy()
    {
        if (!base.ShouldDestroy()) return false;

        return ItemInstance != null && ItemInstance.Amount <= 0;
    }

    protected override LocalizationItem GetName()
    {
        if (itemDefinition == null) return null;

        return itemDefinition.NameLocalizationItem;
    }

    protected override Sprite GetIcon()
    {
        if (itemDefinition == null) return null;

        return itemDefinition.ItemIcon;
    }

    // Item
    public virtual void SetItemInstance(ItemInstance itemInstance)
    {
        if (itemInstance == null) {
            Debug.LogError($"[{nameof(ItemWidget)}] Item Instance is not valid!");
            return;
        }

        if (itemInstance == ItemInstance)
            return;

        ItemInstance = itemInstance;

        SetItemDefinition(itemInstance.Definition);
    }

    public virtual void SetItemDefinition(ItemDefinition itemDefinition)
    {
        if (itemDefinition == null) {
            Debug.LogError($"[{nameof(ItemWidget)}] Item Definition is not valid!");
            return;
        }

        if (itemDefinition == this.itemDefinition)
            return;

        this.itemDefinition = itemDefinition;

        UpdateItemFromCityStorage();
        UpdateName();
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

        var item = cityStorage.Inventory.GetInventoryItem(itemDefinition.ItemId);

        if (item != null)
            SetItemInstance(item);
    }

    public void SetIsCityItem(bool value)
    {
        isCityItem = value;
    }
}