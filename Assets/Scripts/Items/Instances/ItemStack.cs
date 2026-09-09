using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum ItemStackEnum
{
    Population,
    Electricity,
    Food,
    Wood,
    Stone,
    Scrap,
    Plastic,
    Provisions,
    Weapon,
}

[Serializable]
public class ItemStack : IItemAmount
{
    [SerializeField] private ItemStackEnum stackEnum = ItemStackEnum.Population;
    public ItemStackEnum StackEnum => stackEnum;

    [SerializeField, FormerlySerializedAs("limit")]
    private int amount = 0;

    // Stack capacity.
    public int Amount => amount;

    // Items that belong to this stack.
    public List<IItemAmount> ItemAmounts { get; private set; } = new();

    public event Action<int> OnAmountChanged;

    public ItemStack(ItemStackEnum stackEnum)
    {
        this.stackEnum = stackEnum;
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

    public void AddItemAmount(IItemAmount value)
    {
        if (value == null)
            return;

        if (ItemAmounts.Contains(value))
            return;

        ItemAmounts.Add(value);
    }

    public void RemoveItem(IItemAmount value)
    {
        if (value == null)
            return;

        ItemAmounts.Remove(value);
    }

    public int GetItemsAmountSum()
    {
        int sum = 0;

        foreach (var item in ItemAmounts) {
            if (item == null)
                continue;

            sum += Mathf.Max(0, item.Amount);
        }

        return sum;
    }

    private void SetLimit(int value)
    {
        amount = Mathf.Max(0, value);
        OnAmountChanged?.Invoke(amount);
    }
}