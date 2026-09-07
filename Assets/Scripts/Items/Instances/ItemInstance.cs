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
    [SerializeField, FormerlySerializedAs("itemData")]
    private ItemDefinition definition;

    public ItemDefinition Definition => definition;

    [SerializeField]
    private int amount;

    public int Amount => amount;

    public ItemStack Stack { get; private set; }

    public event Action<ItemInstance, int> OnItemAmountAdded;
    public event Action<ItemInstance, int> OnItemAmountRemoved;

    public event Action<ItemInstance> OnItemAmountChanged;
    public event Action<int> OnAmountChanged;

    public ItemInstance(ItemDefinition definition)
    {
        this.definition = definition;
    }

    public virtual void SetAmount(int amount)
    {
        amount = Mathf.Max(0, amount);

        if (Stack != null) {
            int otherItemsAmount = Stack.GetItemsAmountSum() - this.amount;
            int availableAmount = Mathf.Max(0, Stack.Amount - otherItemsAmount);

            amount = Mathf.Min(amount, availableAmount);
        }

        if (this.amount == amount)
            return;

        int lastAmount = this.amount;
        this.amount = amount;

        int difference = Mathf.Abs(amount - lastAmount);

        if (amount > lastAmount) {
            OnItemAmountAdded?.Invoke(this, difference);
        }
        else {
            OnItemAmountRemoved?.Invoke(this, difference);
        }

        OnAmountChanged?.Invoke(this.amount);
        OnItemAmountChanged?.Invoke(this);
    }

    public virtual void AddAmount(int amount)
    {
        if (amount <= 0)
            return;

        SetAmount(this.amount + amount);
    }

    public virtual void RemoveAmount(int amount)
    {
        if (amount <= 0)
            return;

        SetAmount(this.amount - amount);
    }

    public void SetStack(ItemStack stack)
    {
        if (Stack == stack)
            return;

        if (Stack != null)
            Stack.RemoveItemAmount(this);

        Stack = stack;

        if (Stack != null) {
            Stack.AddItemAmount(this);

            SetAmount(amount);
        }
    }

    // Localization
    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            {
                "itemName",
                LocalizationManager.Instance.GetLocalizedText(
                    definition.NameLocalizationItem)
            },
            {
                "itemAmount",
                amount.ToString()
            }
        };
    }

    // Information
    public LocalizationItem GetInformationName()
    {
        if (!Definition)
            return null;

        return Definition.NameLocalizationItem;
    }

    public LocalizationItem GetInformationDescription()
    {
        if (!Definition)
            return null;

        return Definition.DescriptionLocalizationItem;
    }

    public Sprite GetInformationImage()
    {
        if (!Definition)
            return null;

        return Definition.ItemIcon;
    }

    // Factory
    public static ItemInstance Create(ItemData itemData)
    {
        if (itemData == null)
            return null;

        var definition = ItemsList.Instance.GetItem(itemData.Id);

        if (definition == null)
            return null;

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

        var items = new List<ItemInstance>();

        foreach (var data in itemData) {
            var item = Create(data);

            if (item != null)
                items.Add(item);
        }

        return items.ToArray();
    }
}