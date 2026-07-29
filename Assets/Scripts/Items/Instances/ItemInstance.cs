using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ItemCategoryData
{
    [SerializeField] private ItemCategory itemCategory;
    public ItemCategory ItemCategory => itemCategory;

    [SerializeField] private int amount;
    public int Amount => amount;
}

[Serializable]
public class ItemInstance : IItemAmount, ILocalizable, IInformationable
{
    [SerializeField, FormerlySerializedAs("itemData")] private ItemDefinition definition;
    public ItemDefinition Definition => definition;

    [SerializeField] private int amount;
    public int Amount => amount;

    public ItemStack Stack { get; private set; }

    public event Action<int> OnAmountChanged;

    public ItemInstance(ItemDefinition definition)
    {
        this.definition = definition;
    }

    public virtual void SetAmount(int amount)
    {
        this.amount = Mathf.Max(0, amount);
        OnAmountChanged?.Invoke(amount);
    }

    public virtual void AddAmount(int amount)
    {
        SetAmount(this.amount + amount);
    }

    public virtual void RemoveAmount(int amount)
    {
        SetAmount(this.amount - amount);
    }

    public void SetStack(ItemStack stack)
    {
        this.Stack = stack;
    }

    // Localization
    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "itemName", LocalizationManager.Instance.GetLocalizedText(definition.NameLocalizationItem) },
            { "itemAmount", amount.ToString() },
        };
    }

    // Information
    public LocalizationItem GetInformationName()
    {
        if (!Definition) return null;

        return Definition.NameLocalizationItem;
    }

    public LocalizationItem GetInformationDescription()
    {
        if (!Definition) return null;

        return Definition.DescriptionLocalizationItem;
    }

    public Sprite GetInformationImage()
    {
        if (!Definition) return null;

        return Definition.ItemIcon;
    }

    // Factory
    public static ItemInstance Create(ItemData itemData)
    {
        var definition = ItemsList.Instance.GetItem(itemData.Id);

        var item = definition.CreateInstance();
        item.SetAmount(itemData.Amount);

        return item;
    }

    public static ItemInstance[] Create(ItemData[] itemData)
    {
        if (itemData == null) {
            Debug.Log("itemData array not found");
            return null;
        }

        List<ItemInstance> items = new List<ItemInstance>();

        foreach (ItemData item in itemData) {
            items.Add(Create(item));
        }

        return items.ToArray();
    }
}