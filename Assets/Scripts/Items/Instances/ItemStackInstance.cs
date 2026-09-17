using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ItemStackInstance : IAmountable, IInformationable
{
    [SerializeField] private ItemStackDefinition definition;
    public ItemStackDefinition Definition => definition;

    [SerializeField, FormerlySerializedAs("limit")] private int amount = 0;
    public int Amount => amount;

    [SerializeField] private List<ItemInstance> items = new();
    public IReadOnlyList<ItemInstance> Items => items;

    public event Action<int> OnAmountChanged;

    public ItemStackInstance(ItemStackDefinition definition)
    {
        this.definition = definition;
    }

    public void AddLimit(int value)
    {
        if (value <= 0)
            return;

        SetLimit(amount + value);
    }

    public void RemoveLimit(int value)
    {
        if (value <= 0)
            return;

        SetLimit(amount - value);
    }

    public void AddItemAmount(ItemInstance value)
    {
        if (value == null)
            return;

        if (items.Contains(value))
            return;

        items.Add(value);
    }

    public void RemoveItem(ItemInstance value)
    {
        if (value == null)
            return;

        items.Remove(value);
    }

    public int GetItemsAmountSum()
    {
        int sum = 0;

        foreach (var item in items) {
            if (item == null)
                continue;

            sum += Mathf.Max(0, item.Amount);
        }

        return sum;
    }

    // IInformationable
    public LocalizationItem GetInformationName()
    {
        return definition.NameLocalizationItem;
    }

    public LocalizationItem GetInformationDescription()
    {
        return definition.DescriptionLocalizationItem;
    }

    public Sprite GetInformationIcon()
    {
        return definition.Icon;
    }

    private void SetLimit(int value)
    {
        amount = Mathf.Max(0, value);
        OnAmountChanged?.Invoke(amount);
    }
}